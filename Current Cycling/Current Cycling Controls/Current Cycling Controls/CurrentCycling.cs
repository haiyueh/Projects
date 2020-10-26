using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Timers;
using System.IO.Ports;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;

namespace Current_Cycling_Controls {
    public delegate void TDKEvent(object sender, GUIArgs e);
    public class CurrentCycling {
        public event TDKEvent UpdateUI;
        private SerialPort _serTDK;
        private DateTime _cycleTimer = DateTime.Now;
        private Stopwatch _biasTimer;
        private Stopwatch _resultsTimer;
        private Stopwatch _totalTimer;
        private Stopwatch _intoCycleTimer;
        private Stopwatch _voltageTimer;
        private bool _timeOut;
        public List<TDK> _TDK;
        public bool _updateRun;
        public GUIArgs _args;
        public string _resultsDir = $".\\backupData\\";
        public bool STOP, SMOKEALARM, TEMPALARM, GUISTOP, EMSSTOP, OVERMAXVOLTAGE;
        public bool BIASON;
        public bool SAVE;
        public List<double> _temps;
        public List<double> _smokeRaw;
        public List<double> _smokeLevel;
        public List<double> _voltages;
        public event CoreCommandEvent NewCoreCommand;
        public event DataEvent CycleDataEvent;
        private readonly Data _conn = new Data();
        public bool DataError = false;

        public CurrentCycling() {
            CycleDataEvent += _conn.QueueData;
        }

        public void StartCycling(StartCyclingArgs args) {
            _serTDK = new SerialPort();
            var tdk = args.TDK;
            if (tdk.Count < 1) return;
            U.Logger.WriteLine("Waiting for HW interlock");
            NewCoreCommand?.Invoke(this, new CoreCommand() { Type = U.CmdType.UpdateHeartBeatPacket });
            Thread.Sleep(2000); // wait for HW interlock to disengage
            OpenPorts();
            try {
                foreach (var t in tdk) {
                    try {
                        SetAddress(t);
                        SetCurrentVoltage(t);
                        t.Connected = true;
                    }
                    catch (TimeoutException exc) {
                        U.Logger.WriteLine($"TIMEOUT ON PORT #{t.Port}");
                        throw new Exception(exc.Message);
                    }
                }
                // start bias cycle
                foreach (var t in tdk) {
                    t.CycleCount++;
                    t.PastVoltages = new List<string>();
                }
                _TDK = tdk;
                _totalTimer = new Stopwatch();
                _totalTimer.Start();
                _voltageTimer = new Stopwatch();
                _voltageTimer.Start();
                DataError = false; // assume connection is good at start
                // Loop forever until we get a stop command from main thread
                while (true) {
                    _intoCycleTimer = new Stopwatch();
                    _intoCycleTimer.Start();

                    // BIAS ON
                    foreach (var t in tdk) {
                        TurnON(t);
                    }
                    BIASON = true;
                    NewCoreCommand?.Invoke(this, new CoreCommand() { Type = U.CmdType.UpdateHeartBeatPacket });
                    StartTimer();
                    StartResultsTimer();
                    _cycleTimer = DateTime.Now.AddMilliseconds(args.BiasOnTime);
                    while (_biasTimer.ElapsedMilliseconds < args.BiasOnTime
                        && !STOP && !TEMPALARM && !SMOKEALARM) {
                        if (_resultsTimer.ElapsedMilliseconds > U.ResultsSaveTimeON) SAVE = true;
                        foreach (var tt in tdk) {
                            if (STOP || SMOKEALARM || TEMPALARM) break;
                            SetAddress(tt);
                            _serTDK.DiscardInBuffer();

                            tt.Voltage = MeasureVoltage();
                            tt.Current = MeasureCurrent();
                            _args = new GUIArgs(tt.Voltage, tt.Current, tt.CycleCount, tt.Port, _cycleTimer);
                            NewCoreCommand?.Invoke(this, new CoreCommand() { Type = U.CmdType.UpdateUI });
                            if (SAVE) {
                                SaveResultsBQ(tt);
                            }
                            
                            if (double.Parse(tt.Voltage) > args.MaxOverVoltage) {
                                STOP = true;
                                OVERMAXVOLTAGE = true;
                            } 
                        }
                        if (STOP || SMOKEALARM || TEMPALARM) break;
                        // if we have saved then restart timer
                        if (SAVE) {
                            SAVE = false;
                            _resultsTimer.Restart();
                        }
                    }
                    if (STOP || SMOKEALARM || TEMPALARM) break;
                    // BIAS OFF
                    TurnOff(tdk);
                    BIASON = false;
                    NewCoreCommand?.Invoke(this, new CoreCommand() { Type = U.CmdType.UpdateHeartBeatPacket });
                    StartTimer();
                    StartResultsTimer();
                    _cycleTimer = DateTime.Now.AddMilliseconds(args.BiasOffTime);
                    while (_biasTimer.ElapsedMilliseconds < args.BiasOffTime
                        && !STOP && !TEMPALARM && !SMOKEALARM) {
                        if (_resultsTimer.ElapsedMilliseconds > U.ResultsSaveTimeOFF) SAVE = true;
                        foreach (var tt in tdk) {
                            if (STOP || SMOKEALARM || TEMPALARM) break;
                            tt.Voltage = MeasureVoltage();
                            tt.Current = MeasureCurrent();
                            _args = new GUIArgs(tt.Voltage, tt.Current, tt.CycleCount, tt.Port, _cycleTimer);
                            NewCoreCommand?.Invoke(this, new CoreCommand() { Type = U.CmdType.UpdateUI });
                            if (SAVE) {
                                SaveResultsBQ(tt);
                            }
                        }
                        if (STOP || SMOKEALARM || TEMPALARM) break;

                        // if we have saved then restart timer
                        if (SAVE) {
                            SAVE = false;
                            _resultsTimer.Restart();
                        }

                    }
                    if (STOP || SMOKEALARM || TEMPALARM) break;

                    // completed a bias on/off cycle
                    foreach (var ttt in tdk) {
                        ttt.CycleCount++;
                    }
                }
            }
            catch (Exception exc) {
                U.Logger.WriteLine($"{exc}");
                BIASON = false;
                STOP = false;
                SMOKEALARM = false;
                TEMPALARM = false;
                TurnOffClose(tdk);
                U.Logger.WriteLine($"TDK closing with exception");

            }
            var cmd = EMSSTOP ? "EMSSTOP" : SMOKEALARM ? "SMOKEALARM" : GUISTOP ? "GUI Stop" : OVERMAXVOLTAGE ? "Over Max Voltage" : "TEMPALARM";
            U.Logger.WriteLine($"Encountered {cmd} while Cycling!");
            BIASON = false;
            STOP = false;
            SMOKEALARM = false;
            TEMPALARM = false;
            EMSSTOP = false;
            GUISTOP = false;
            OVERMAXVOLTAGE = false;
            TurnOffClose(tdk);
            U.Logger.WriteLine($"TDK Closing Normally");
        }

        private void SaveResultsBQ(TDK t) {
            var point = CompileCCData(t);
            CycleDataEvent?.Invoke(this, new DataQueueObject(DataQueueObject.DataType.CycleData, point));
        }

        private CCDataPoint CompileCCData(TDK t) {
            return new CCDataPoint(t.CycleCount,
                (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds, // epoch
                (_totalTimer.ElapsedMilliseconds / 3.6E+6), // total time (hrs)
                (_intoCycleTimer.ElapsedMilliseconds / 60000.0), // time into current cycle
                BIASON, t.SampleName, double.Parse(t.Current), double.Parse(t.Voltage), int.Parse(t.NumCells),
                double.Parse(t.Voc), int.Parse(t.TempSensor), double.Parse(t.SetCurrent), -99.99,
                _temps, _smokeRaw, _smokeLevel);
        }

        private void SaveResults(TDK t) {
            var str = CompileDataStr(t);
            var path = _resultsDir + $"{t.SampleName}_{DateTime.Now:M-dd--HH-mm--ss}.txt";
            using (var writer = new StreamWriter(path, true)) {
                writer.WriteLine(str);
            }
        }

        private string CompileDataStr(TDK t) {
            string[] str = { t.CycleCount.ToString(), (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds.ToString(),
                (_totalTimer.ElapsedMilliseconds / 3.6E+6).ToString(), // total time (hrs)
                (_intoCycleTimer.ElapsedMilliseconds / 60000.0).ToString(), // time into current cycle
                (BIASON) ? "ON" : "OFF",
                t.SampleName, t.Current, t.Voltage, t.NumCells, t.Voc,
                t.TempSensor, t.SetCurrent, "-99.99",
                $"{_temps[0].ToString("F2")}",$"{_temps[1].ToString("F2")}",$"{_temps[2].ToString("F2")}",
                $"{_temps[3].ToString("F2")}",$"{_temps[4].ToString("F2")}",$"{_temps[5].ToString("F2")}",
                $"{_temps[6].ToString("F2")}",$"{_temps[7].ToString("F2")}",$"{_temps[8].ToString("F2")}",
                $"{_temps[9].ToString("F2")}",$"{_temps[10].ToString("F2")}",$"{_temps[11].ToString("F2")}",
                $"{_temps[12].ToString("F2")}",$"{_temps[13].ToString("F2")}",$"{_temps[14].ToString("F2")}",
                $"{_temps[15].ToString("F2")}",
                $"{_smokeRaw[0].ToString("F2")}", $"{_smokeRaw[1].ToString("F2")}", $"{_smokeRaw[2].ToString("F2")}",
                $"{_smokeRaw[3].ToString("F2")}", $"{_smokeRaw[4].ToString("F2")}", $"{_smokeRaw[5].ToString("F2")}",
                $"{_smokeRaw[6].ToString("F2")}", $"{_smokeRaw[7].ToString("F2")}",
                $"{_smokeLevel[0].ToString("F2")}", $"{_smokeLevel[1].ToString("F2")}", $"{_smokeLevel[2].ToString("F2")}",
                $"{_smokeLevel[3].ToString("F2")}", $"{_smokeLevel[4].ToString("F2")}", $"{_smokeLevel[5].ToString("F2")}",
                $"{_smokeLevel[6].ToString("F2")}", $"{_smokeLevel[7].ToString("F2")}"};
            return string.Join(",", str);
        }

        private void OpenPorts() {
            string[] ports = SerialPort.GetPortNames();
            // ping each port and see if we get the TDK response
            foreach (var port in ports) {
                try {
                    _serTDK.BaudRate = U.TDKBaudRate;
                    _serTDK.PortName = port; // com3
                    _serTDK.NewLine = "\r";
                    _serTDK.ReadTimeout = 1000;
                    _serTDK.Open();

                    _serTDK.Write("ADR " + "01" + "\r\n");
                    if (_serTDK.ReadLine() == "OK") {
                        _serTDK.DiscardOutBuffer();
                        _serTDK.DiscardInBuffer();
                        return;
                    }
                }
                catch (Exception exc) { _serTDK.Close(); }
            }
        }

        private string MeasureVoltage() {
            string val = "";
            bool stop = false;
            while (!stop) {
                _serTDK.Write("MV?\r\n");
                Wait(50); // lag in measured value
                val = _serTDK.ReadLine();
                // get rid of OK packets and check if value is parsable
                if (double.TryParse(val, out var num)) stop = true;
            }
            return val;
            
        }

        private string MeasureCurrent() {
            string val = "";
            bool stop = false;
            _serTDK.DiscardInBuffer();
            while (!stop) {
                _serTDK.Write("MC?\r\n");
                Wait(50); // lag in measured value
                val = _serTDK.ReadLine();
                if (double.TryParse(val, out var num)) stop = true;
            }
            return val;

        }

        private void SetAddress(TDK tdk) {
            // Sets the address of the power supply
            _serTDK.Write("ADR " + tdk.Address + "\r\n");
            if (_serTDK.ReadLine() == "OK") { }
            _serTDK.DiscardInBuffer();
        }

        private void SetCurrentVoltage(TDK tdk) {
            // Sets the current limit of the power supply
            do {
                _serTDK.Write("PC " + tdk.SetCurrent + "\r\n");
                if (_serTDK.ReadLine() == "OK") {
                    U.Logger.WriteLine($"Current: OKAY");
                }
                _serTDK.Write("PC?\r\n");
            } while (_serTDK.ReadLine() == tdk.SetCurrent);

            do {
                //Sets the voltage of the power supply
                _serTDK.Write("PV " + U.VoltageCompliance + "\r\n");
                if (_serTDK.ReadLine() == "OK") {
                    U.Logger.WriteLine($"Voltage: OKAY");
                }
                _serTDK.Write("PC?\r\n");
            } while (_serTDK.ReadLine() == U.VoltageCompliance);
            _serTDK.DiscardInBuffer();
        }

        private void TurnON(TDK tdk) {
            // Sets the address of the power supply
            SetAddress(tdk);

            do {
                _serTDK.Write("OUT ON\r\n");
                if (_serTDK.ReadLine() == "OK") {
                    U.Logger.WriteLine($"ON: OKAY");
                }
                _serTDK.Write("MODE?\r\n");
            } while (_serTDK.ReadLine() == "ON");
            _serTDK.DiscardInBuffer();
        }

        private void TurnOff(List<TDK> tdk) {
            foreach (var t in tdk) {
                _serTDK.Write("ADR " + t.Address + "\r\n");
                _serTDK.Write("OUT OFF\r\n");
                if (_serTDK.ReadLine() == "OK") {
                    U.Logger.WriteLine($"OFF: OKAY");
                }
                _serTDK.DiscardOutBuffer();
                _serTDK.DiscardInBuffer();
            }
        }

        private void TurnOffClose(List<TDK> tdk) {
            foreach (var t in tdk) {
                _serTDK.Write("ADR " + t.Address + "\r\n");
                _serTDK.Write("OUT OFF\r\n");
                if (_serTDK.ReadLine() == "OK") {
                    U.Logger.WriteLine($"Port #{t.Port} OFF: OKAY");
                }
                _serTDK.DiscardOutBuffer();
                _serTDK.DiscardInBuffer();


            }
            StartTimer();
            // wait and get updated measured value from TDKs for the GUI
            Wait(500);
            foreach (var tt in tdk) {
                string volt = MeasureVoltage();
                string current = MeasureCurrent();
                _args = new GUIArgs(volt, current, tt.CycleCount, tt.Port, _cycleTimer, closing: true);
                NewCoreCommand?.Invoke(this, new CoreCommand() { Type = U.CmdType.UpdateUI });
            }
            _serTDK.Close();
        }

        private void StartTimer() {
            _biasTimer = new Stopwatch();
            _biasTimer.Start();
        }

        private void StartResultsTimer() {
            _resultsTimer = new Stopwatch();
            _resultsTimer.Start();
        }

        private void Wait(int t) {
            long elapsed = _biasTimer.ElapsedMilliseconds;
            while (_biasTimer.ElapsedMilliseconds - elapsed < t) { }
        }

        private void WaitResults(int t) {
            long elapsed = _resultsTimer.ElapsedMilliseconds;
            while (_resultsTimer.ElapsedMilliseconds - elapsed < t) { }
        }

        private void GraphVoltages(TDK t) {
            Chart chart1 = new Chart();
            chart1.Series.Clear();
            var series = new Series("Volts");
            series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series.Enabled = true;
            chart1.Series.Add(series);
            chart1.Visible = true;
            ChartArea chA = new ChartArea();
            chA.AxisX.RoundAxisValues();
            chA.AxisX.Title = "Cycle";
            chA.AxisY.Title = "Voltage";
            chart1.ChartAreas.Add(chA);
            var lin = Linespace(1, t.CycleCount, t.PastVoltages.Count - 2);
            var ii = 0;
            foreach (var val in t.PastVoltages) {
                chart1.Series["Volts"].Points.AddXY(lin[ii], val);
                ii++;
            }
            chart1.Invalidate();

            chart1.SaveImage($"{_resultsDir}\\charts\\{t.SampleName}_C{t.CycleCount}.png", System.Drawing.Imaging.ImageFormat.Png);

        }

        /// <summary>
        /// Mimic MatLab's LinSpace function (:
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="sections"></param>
        /// <returns></returns>
        private List<double> Linespace(double start, double end, int sections) {
            List<double> lst = new List<double>();
            for (var i = 0; i < sections + 2; i++) {
                lst.Add(start + ((end - start) / (sections + 1)) * i);
            }
            return lst;
        }

    }

    public class GUIArgs {
        public string Volt { get; set; }
        public string Current { get; set; }
        public string Cycle { get; set; }
        public DateTime CycleTime { get; set; }
        public int Port { get; set; }
        public bool Closing { get; set; }
        public GUIArgs(string volt, string current, int cycle, int port, DateTime dt, bool closing = false) {
            Volt = volt;
            Current = current;
            Cycle = cycle.ToString();
            Port = port;
            CycleTime = dt;
            Closing = closing;
        }
    }

}
