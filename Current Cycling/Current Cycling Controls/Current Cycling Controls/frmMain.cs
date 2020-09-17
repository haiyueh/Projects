using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.Windows.Forms.DataVisualization.Charting;
using System.Diagnostics;
using System.Timers;

namespace Current_Cycling_Controls {
    public delegate void HeartBeatUpdate(object sender, TransmitPacket t);
    public partial class frmMain : Form {
        public bool Connected { get; set; }
        public HeartBeatUpdate heartBeatUpdates;
        private readonly AutoResetEvent _commReset = new AutoResetEvent(false);
        private BackgroundWorker _commWorker = new BackgroundWorker();
        private BackgroundWorker _TDKWorker = new BackgroundWorker();
        private BackgroundWorker _arduinoWorker = new BackgroundWorker();
        private BackgroundWorker _connectionWorker = new BackgroundWorker();
        private readonly Queue<CoreCommand> _commandQueue = new Queue<CoreCommand>();
        private CurrentCycling _cycling = new CurrentCycling();
        private ArduinoMachine _arduino = new ArduinoMachine();
        private readonly object _lock = new object();
        private List<TDK> _TDKS;
        private List<CheckBox> _checkBoxes;
        private List<Label> _samples;
        private List<TextBox> _tempSensors;
        private List<TextBox> _setCurrents;
        private List<Label> _tempLabels;
        private List<Label> _smokeLabels;
        private List<Label> _voltageLabels;
        private List<Label> _currentLabels;
        private List<Label> _cycleLabels;
        private List<Label> _connectedLabels;
        private List<Button> _loadButtons;
        private List<Button> _newButtons;
        private List<TextBox> _voc;
        private List<TextBox> _numCells;
        private List<bool> _TDKconnection;
        private List<double> _smokeLevel;
        private List<double> _refSmokes;
        private int yy = 0;
        private string Cycling;
        private bool SMOKEALARM;
        public event DataEvent TestDataEvent;
        private readonly Data BQConn = new Data();
        private CCRecipe _ccRecipe;

        private DateTime _cycleTimer = DateTime.Now;
        private TransmitPacket _heartBeatPacket;
        public frmMain() {
            InitializeComponent();
            Connected = false;
            this.FormClosing += new FormClosingEventHandler(Form_Closing);
            TestDataEvent += BQConn.QueueData;
            Directory.CreateDirectory("logs");
            Directory.CreateDirectory("backupData");
            _commWorker.DoWork += RunCommMachine;
            _commWorker.WorkerReportsProgress = true;
            _commWorker.ProgressChanged += UpdateUi;
            _commWorker.RunWorkerAsync();

            _TDKWorker.DoWork += RunCurrentCycling;
            _TDKWorker.RunWorkerCompleted += CyclingComplete;

            _arduinoWorker.DoWork += RunArduinoLoop;
            _arduinoWorker.WorkerReportsProgress = true;
            _arduinoWorker.ProgressChanged += UpdateHeartBeat;
            _arduinoWorker.RunWorkerAsync();

            _connectionWorker.DoWork += CheckConnect;
            _connectionWorker.RunWorkerCompleted += ConnectComplete;

            _cycling.NewCoreCommand += NewCoreCommand;
            _arduino.NewCoreCommand += NewCoreCommand;

            
            _refSmokes = new List<double> { 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000 };
            _smokeLevel = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0 };
            _TDKconnection = new List<bool> { false, false, false, false, false, false,
                false, false, false, false, false, false, };
            _samples = new List<Label> { lblSample1, lblSample2, lblSample3, lblSample4,
            lblSample5,lblSample6,lblSample7,lblSample8,lblSample9,lblSample10,lblSample11,lblSample12};
            _checkBoxes = new List<CheckBox> { chkbxPort1 , chkbxPort2, chkbxPort3,
            chkbxPort4, chkbxPort5, chkbxPort6, chkbxPort7, chkbxPort8, chkbxPort9,
            chkbxPort10, chkbxPort11, chkbxPort12};
            _setCurrents = new List<TextBox> { txtSetCurr1, txtSetCurr2, txtSetCurr3,
            txtSetCurr4,txtSetCurr5,txtSetCurr6,txtSetCurr7,txtSetCurr8,txtSetCurr9,
            txtSetCurr10,txtSetCurr11,txtSetCurr12};
            _tempSensors = new List<TextBox> { txtTempSensSample1, txtTempSensSample2,
            txtTempSensSample3,txtTempSensSample4,txtTempSensSample5,txtTempSensSample6,
            txtTempSensSample7,txtTempSensSample8,txtTempSensSample9,
            txtTempSensSample10,txtTempSensSample11,txtTempSensSample12};
            //_tempLabels = new List<Label> { labelTemp1, labelTemp2,
            //labelTemp3,labelTemp4,labelTemp5,labelTemp6,
            //labelTemp7,labelTemp8,labelTemp9,labelTemp10,
            //labelTemp11,labelTemp12, labelTemp13, labelTemp14, labelTemp15, labelTemp16};
            //_smokeLabels = new List<Label> { labelSmoke1, labelSmoke2,
            //labelSmoke3,labelSmoke4,labelSmoke5,labelSmoke6,
            //labelSmoke7,labelSmoke8};
            _voltageLabels = new List<Label> { lblVoltage1, lblVoltage2 , lblVoltage3 ,
                lblVoltage4,lblVoltage5,lblVoltage6,lblVoltage7,lblVoltage8,lblVoltage9,
                lblVoltage10,lblVoltage11,lblVoltage12};
            _currentLabels = new List<Label> { lblCurrent1, lblCurrent2 , lblCurrent3 ,
                lblCurrent4,lblCurrent5,lblCurrent6,lblCurrent7,lblCurrent8,lblCurrent9,
                lblCurrent10,lblCurrent11,lblCurrent12};
            _cycleLabels = new List<Label> { lblCycle1, lblCycle2, lblCycle3,
                lblCycle4,lblCycle5,lblCycle6,lblCycle7,lblCycle8,lblCycle9,
                lblCycle10,lblCycle11,lblCycle12};
            _connectedLabels = new List<Label> { lblPSStatus1, lblPSStatus2, lblPSStatus3,
            lblPSStatus4,lblPSStatus5,lblPSStatus6,lblPSStatus7,lblPSStatus8,lblPSStatus9,
            lblPSStatus10,lblPSStatus11,lblPSStatus12 };
            _loadButtons = new List<Button> { btnLoad1, btnLoad2, btnLoad3, btnLoad4,
            btnLoad5,btnLoad6,btnLoad7,btnLoad8,btnLoad9,btnLoad10,btnLoad11,btnLoad12};
            //_newButtons = new List<Button> { btnNew1, btnNew2, btnNew3 , btnNew4 ,
            //btnNew5,btnNew6,btnNew7,btnNew8,btnNew9,btnNew10,btnNew11,btnNew12};
            _voc = new List<TextBox> { txtVoc1, txtVoc2 , txtVoc3 , txtVoc4 , txtVoc5 ,
            txtVoc6,txtVoc7,txtVoc8,txtVoc9,txtVoc10,txtVoc11,txtVoc12};
            _numCells = new List<TextBox> { txtNumCells1, txtNumCells2 , txtNumCells3 ,
            txtNumCells4,txtNumCells5,txtNumCells6,txtNumCells7,txtNumCells8,txtNumCells9
            ,txtNumCells10,txtNumCells11,txtNumCells12};

            // reload default settings to GUI
            txtOperator.Text = Properties.Settings.Default.Operator;
            txtBiasOn.Text = Properties.Settings.Default.BiasON;
            txtBiasOff.Text = Properties.Settings.Default.BiasOFF;
            txtCurrOnTempSet.Text = Properties.Settings.Default.BiasONTempSet;
            txtCurrOffTempSet.Text = Properties.Settings.Default.BiasOFFTempSet;
            txtOverTempSet.Text = Properties.Settings.Default.OverTempSet;
            txtSmokeOverSet.Text = Properties.Settings.Default.OverSmokeSet;
            txtPauseFans.Text = Properties.Settings.Default.PauseFanTime;
            if (Properties.Settings.Default.CheckBoxes != null) {
                var iii = 0;
                var chks = Properties.Settings.Default.CheckBoxes;
                foreach (var chk in _checkBoxes) {
                    chk.Checked = chks[iii];
                    iii++;
                }
            }
            else {
                Properties.Settings.Default.CheckBoxes = new List<bool> { false, false, false, false, false, false, false, false, false, false, false, false };
            }
            if (Properties.Settings.Default.ActiveTemps != null) {
                var temps = chkTemp.Items;
                var ii = 0;
                foreach (var temp in Properties.Settings.Default.ActiveTemps) {
                    chkTemp.SetItemChecked(temps.IndexOf(temps[ii]), temp);
                    ii++;
                }
            }
            else {
                Properties.Settings.Default.ActiveTemps = new List<bool> { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
            }
            if (Properties.Settings.Default.ActiveSmokes != null) {
                var smokes = chkSmoke.Items;
                var ii = 0;
                foreach (var smoke in Properties.Settings.Default.ActiveSmokes) {
                    chkSmoke.SetItemChecked(smokes.IndexOf(smokes[ii]), smoke);
                    ii++;
                }
            }
            else {
                Properties.Settings.Default.ActiveSmokes = new List<bool> { false, false, false, false, false, false, false, false };
            }
            if (Properties.Settings.Default.Samples == null) {
                Properties.Settings.Default.Samples = new List<string> { null, null, null, null, null, null, null, null, null, null, null, null };
                btnLoadSamples.Enabled = false;
            }
            Properties.Settings.Default.Save();

            // set TDK rows to disabled until the connnection is good
            EnableTDKRows();

            // initialize TDK objects
            _TDKS = new List<TDK> { };
            for (int j = 1; j < 13; j++) {
                _TDKS.Add(new TDK("0" + j, j));
            }

            // initialize heartbeatpacket before arduino declarations
            string tempBin = "";
            foreach (object chk in chkTemp.Items) {
                tempBin += GetBinary(chkTemp.GetItemChecked(chkTemp.Items.IndexOf(chk)));
            }
            string smokeBin = "";
            foreach (object chk in chkSmoke.Items) {
                smokeBin += GetBinary(chkTemp.GetItemChecked(chkTemp.Items.IndexOf(chk)));
            }
            _heartBeatPacket = new TransmitPacket(txtOverTempSet.Text, txtSmokeOverSet.Text,
                txtCurrOnTempSet.Text, txtCurrOffTempSet.Text, "0", "0", tempBin, smokeBin, "0");

            // initialize temp/smoke graphs
            chartTemp.ChartAreas["ChartArea1"].AxisY.Maximum = double.Parse(txtOverTempSet.Text) + 30;
            chartTemp.ChartAreas["ChartArea1"].AxisY.StripLines.Clear();
            StripLine stripline = new StripLine();
            stripline.Interval = 0;
            stripline.IntervalOffset = double.Parse(txtOverTempSet.Text);
            stripline.StripWidth = 3;
            stripline.BackColor = Color.Red;
            chartTemp.ChartAreas["ChartArea1"].AxisY.StripLines.Add(stripline);
            var i = 1;
            foreach (object chk in chkTemp.Items) {
                if (chkTemp.GetItemChecked(chkTemp.Items.IndexOf(chk))) {
                    var y = 25;
                    chartTemp.Series["Temp"].Points.AddXY(i, y);
                    i++;
                }
            }
            chartSmoke.ChartAreas["ChartArea1"].AxisY.Maximum = double.Parse(txtSmokeOverSet.Text) + 1;
            chartSmoke.ChartAreas["ChartArea1"].AxisY.StripLines.Clear();
            stripline = new StripLine();
            stripline.Interval = 0;
            stripline.IntervalOffset = double.Parse(txtSmokeOverSet.Text);
            stripline.StripWidth = 3;
            stripline.BackColor = Color.Red;
            chartSmoke.ChartAreas["ChartArea1"].AxisY.StripLines.Add(stripline);
            i = 1;
            foreach (object chk in chkSmoke.Items) {
                if (chkSmoke.GetItemChecked(chkSmoke.Items.IndexOf(chk))) {
                    var y = 25;
                    chartSmoke.Series["Smoke Level"].Points.AddXY(i, y);
                    i++;
                }
            }

            // wait until arduino is connected to start connecting TDKs
            //#if !DEBUG
            //while (!Connected) { }
            Thread.Sleep(6000);
            //#endif
            _arduinoWorker.ReportProgress(1);
            _connectionWorker.RunWorkerAsync();
            U.Logger.WriteLine($"Checking TDK connections");
            btnStart.Enabled = false;
            btnCheckConnection.Enabled = false;

#if DEBUG
            var timer = new System.Timers.Timer();
            timer.Elapsed += OnTimedEvent;
            timer.Interval = 1000;
            timer.Start();
#endif
        }


        private void RunCurrentCycling(object s, DoWorkEventArgs e) {
            var tdk = (StartCyclingArgs)e.Argument;
            Cycling = "1";
            _cycling.StartCycling(tdk);
        }

        private void CyclingComplete(object s, RunWorkerCompletedEventArgs e) {
            // clean up TDKs and maybe graph/show output results to file?
            Cycling = "0";
            NewCoreCommand(this, new CoreCommand { Type = U.CmdType.UpdateHeartBeatPacket });
            NewCoreCommand(this, new CoreCommand { Type = U.CmdType.CleanGUI });
            foreach (var t in _TDKS) {
                t.Cycling = false;
            }
        }

        private void RunArduinoLoop(object s, DoWorkEventArgs e) {
            _arduino.StartArduinoMachine();
        }

        public void UpdateHeartBeat(object sender, ProgressChangedEventArgs e) {
            _arduino.UpdateTransmit(_heartBeatPacket, checkBoxPrintPacket.Checked);
        }

        public void CheckConnect(object sender, DoWorkEventArgs e) {
            var labels = CheckConnection();
            e.Result = labels;
        }

        private void ConnectComplete(object s, RunWorkerCompletedEventArgs e) {
            var res = e.Result;
            _commWorker.ReportProgress(4, res);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private CoreCommand GetNextCommand() {
            CoreCommand cmd = null;
            lock (_lock) {
                if (_commandQueue.Count > 0) {
                    cmd = _commandQueue.Dequeue();
                }
            }
            return cmd;
        }
        /// <summary>
        /// Wait for Command, if command append it to command queue and FIFO the commands.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunCommMachine(object sender, DoWorkEventArgs e) {
            while (true) {
                try {
                    _commReset.WaitOne(-1);
                    var cmd = GetNextCommand();
                    while (cmd != null) {
                        HandleCommand(cmd);
                        cmd = GetNextCommand();
                    }
                }
                catch (Exception exc) {
                }
            }
        }

        /// <summary>
        /// Handles Core commands from the Controls modules or the GUI. 
        /// </summary>
        private void HandleCommand(CoreCommand c) {
            if (c == null) {
                Info("Got null command");
                return;
            }
            switch (c.Type) {
                case U.CmdType.None:
                    break;
                case U.CmdType.StartCycling:
                    U.Logger.WriteLine($"Starting TDK Worker thread");
                    NewCoreCommand(this, new CoreCommand { Type = U.CmdType.UpdateHeartBeatPacket });
                    _TDKWorker.RunWorkerAsync(c.StartArgs);
                    break;
                case U.CmdType.UpdateUI:
                    _commWorker.ReportProgress(5, c.StartArgs);
                    break;
                case U.CmdType.StopCycling:
                    _cycling.STOP = true;
                    _cycling.GUISTOP = true;
                    break;
                case U.CmdType.CleanGUI:
                    _commWorker.ReportProgress(1);
                    break;
                case U.CmdType.RecievedPacket:
                    // update GUI with temp/smoke/alarms
                    _commWorker.ReportProgress(2, c.ArduinoArgs);

                    // if cycling is running then update alarms/data
                    var packet = _arduino._recievedPacket;
                    if (_TDKWorker.IsBusy) {
                        //_cycling.SMOKEALARM = packet.SmokeAlarm;
                        _cycling.TEMPALARM = packet.TempAlarm;
                        _cycling.STOP = packet.EMSSTOP;
                        _cycling.EMSSTOP = packet.EMSSTOP;
                        _cycling._temps = new List<double>(packet.TempList);
                        _cycling._smokeRaw = new List<double>(packet.SmokeList);

                    }

                    // update refs with smallest value then check over smoke
                    UpdateSmokeRefs(packet.SmokeList);
                    _cycling._smokeLevel = new List<double>(_smokeLevel);
                    if (CheckSmokeOver(packet.SmokeList)) {
                        _cycling.SMOKEALARM = true;
                        SMOKEALARM = true;
                        U.Logger.WriteLine($"SMOKE ALARM frmMAIN!");
                    }
                    if (SMOKEALARM) {// || packet.TempAlarm || packet.EMSSTOP) {
                        SoundPlayer audio = new SoundPlayer(Properties.Resources.AircraftAlarm);
                        audio.Play();
                    }


                    break;
                case U.CmdType.UpdateHeartBeatPacket:
                    _commWorker.ReportProgress(3);
                    break;
                case U.CmdType.CheckConnection:
                    _connectionWorker.RunWorkerAsync();
                    break;
                case U.CmdType.ArduinoConnectSuccess:
                    Connected = true;
                    break;
            }
        }

        private void NewCoreCommand(object sender, CoreCommand c) {
            lock (_lock) {
                _commandQueue.Enqueue(c);
                _commReset.Set();
            }
        }


        private void UpdateUi(object sender, ProgressChangedEventArgs e) {
            try {
                // update TDK readings during Cycling
                if (e.ProgressPercentage == 5) {
                    var args = _cycling._args;
                    if (args.Closing) {
                        labelCount.Text = $@"00:00";
                        lblBiasStatus.Text = _cycling.BIASON ? "BIAS ON" : "BIAS OFF";
                    }
                    else {
                        var ts = (args.CycleTime - DateTime.Now);
                        labelCount.Text = $@"{ts.Minutes:D2}:{ts.Seconds:D2}";
                        lblBiasStatus.Text = _cycling.BIASON ? "BIAS ON" : "BIAS OFF";
                    }
                    _voltageLabels[args.Port - 1].Text = args.Volt;
                    _currentLabels[args.Port - 1].Text = args.Current;
                    _cycleLabels[args.Port - 1].Text = args.Cycle;
                    return;
                }
                // re-enable GUI buttons
                else if (e.ProgressPercentage == 1) {
                    EnableTDKRows();
                    btnStart.Enabled = true;
                    chkTemp.Enabled = true;
                    chkSmoke.Enabled = true;
                    btnCheckConnection.Enabled = true;
                    btnLoadSamples.Enabled = true;
                    btnClearSamples.Enabled = true;
                    buttonClearAlarms.Enabled = true;
                    return;
                }
                // update temp/smoke/alarm readings
                else if (e.ProgressPercentage == 2) {
                    var ardArgs = _arduino._recievedPacket;

                    var i = 0;
                    chartTemp.ChartAreas["ChartArea1"].AxisY.Maximum = double.Parse(txtOverTempSet.Text) + double.Parse(txtOverTempSet.Text) / 10;
                    chartTemp.ChartAreas["ChartArea1"].AxisY.StripLines.Clear();
                    StripLine stripline = new StripLine();
                    stripline.Interval = 0;
                    stripline.IntervalOffset = double.Parse(txtOverTempSet.Text);
                    stripline.StripWidth = stripline.IntervalOffset / 70;
                    stripline.BackColor = Color.Red;
                    chartTemp.ChartAreas["ChartArea1"].AxisY.StripLines.Add(stripline);
                    chartTemp.Series["Temp"].Points.Clear();
                    foreach (object chk in chkTemp.Items) {
                        if (chkTemp.GetItemChecked(chkTemp.Items.IndexOf(chk))) {
                            var y = ardArgs.TempList[i];
                            chartTemp.Series["Temp"].Points.AddXY(chkTemp.Items.IndexOf(chk) + 1, y);
                            i++;
                        }
                    }
                    i = 0;
                    chartSmoke.ChartAreas["ChartArea1"].AxisY.Maximum = double.Parse(txtSmokeOverSet.Text) + double.Parse(txtSmokeOverSet.Text) / 10;
                    chartSmoke.ChartAreas["ChartArea1"].AxisY.StripLines.Clear();
                    stripline = new StripLine();
                    stripline.Interval = 0;
                    stripline.IntervalOffset = double.Parse(txtSmokeOverSet.Text);
                    stripline.StripWidth = stripline.IntervalOffset / 70;
                    stripline.BackColor = Color.Red;
                    chartSmoke.ChartAreas["ChartArea1"].AxisY.StripLines.Add(stripline);
                    chartSmoke.Series["Smoke Level"].Points.Clear();
                    foreach (object chk in chkSmoke.Items) {
                        if (chkSmoke.GetItemChecked(chkSmoke.Items.IndexOf(chk))) {
                            //var y = ardArgs.SmokeList[i];
                            var y = _smokeLevel[i];
                            chartSmoke.Series["Smoke Level"].Points.AddXY(chkSmoke.Items.IndexOf(chk) + 1, y);
                            i++;
                        }
                    }

                    labelTempAlarm.BackColor = ardArgs.TempAlarm ? Color.Red : Color.Empty;
                    labelSmokeAlarm.BackColor = SMOKEALARM ? Color.Red : Color.Empty;
                    labelEMSStop.BackColor = ardArgs.EMSSTOP ? Color.Red : Color.Empty;
                }
                // send event to arduino thread to update serial transmit packet
                else if (e.ProgressPercentage == 3) {
                    string tempBin = "";
                    foreach (object chk in chkTemp.Items) {
                        tempBin += GetBinary(chkTemp.GetItemChecked(chkTemp.Items.IndexOf(chk)));
                    }
                    string smokeBin = "";
                    foreach (object chk in chkSmoke.Items) {
                        smokeBin += GetBinary(chkSmoke.GetItemChecked(chkSmoke.Items.IndexOf(chk)));
                    }
                    string biasON = _cycling.BIASON ? "1" : "0";
                    if (_TDKWorker.IsBusy) Cycling = "1";
                    _heartBeatPacket = new TransmitPacket(txtOverTempSet.Text, txtSmokeOverSet.Text,
                        txtCurrOnTempSet.Text, txtCurrOffTempSet.Text, biasON, "0", tempBin, smokeBin, Cycling);
                    _arduinoWorker.ReportProgress(1);
                }
                // update connection strings from connection worker
                else if (e.ProgressPercentage == 4) {
                    var res = (List<string>)e.UserState;
                    int i = 0;
                    foreach (string str in res) {
                        _connectedLabels[i].Text = str;
                        i++;
                    }
                    EnableTDKRows();
                    btnCheckConnection.Enabled = true;
                    btnStart.Enabled = true;
                }
                // update smoke/temp charts TESTING
                else if (e.ProgressPercentage == 6) {
                    chartTemp.ChartAreas["ChartArea1"].AxisY.Maximum = double.Parse(txtOverTempSet.Text) + double.Parse(txtOverTempSet.Text) / 10;
                    chartTemp.ChartAreas["ChartArea1"].AxisY.StripLines.Clear();
                    StripLine stripline = new StripLine();
                    stripline.Interval = 0;
                    stripline.IntervalOffset = double.Parse(txtOverTempSet.Text);
                    stripline.StripWidth = stripline.IntervalOffset / 70;
                    stripline.BackColor = Color.Red;
                    chartTemp.ChartAreas["ChartArea1"].AxisY.StripLines.Add(stripline);
                    var i = 1;
                    chartTemp.Series["Temp"].Points.Clear();
                    foreach (object chk in chkTemp.Items) {
                        if (chkTemp.GetItemChecked(chkTemp.Items.IndexOf(chk))) {
                            chartTemp.Series["Temp"].Points.AddXY(chkTemp.Items.IndexOf(chk) + 1, yy);
                            i++;
                        }
                    }
                    chartSmoke.ChartAreas["ChartArea1"].AxisY.Maximum = double.Parse(txtSmokeOverSet.Text) + double.Parse(txtSmokeOverSet.Text) / 10;
                    chartSmoke.ChartAreas["ChartArea1"].AxisY.StripLines.Clear();
                    stripline = new StripLine();
                    stripline.Interval = 0;
                    stripline.IntervalOffset = double.Parse(txtSmokeOverSet.Text);
                    stripline.StripWidth = stripline.IntervalOffset / 70;
                    stripline.BackColor = Color.Red;
                    chartSmoke.ChartAreas["ChartArea1"].AxisY.StripLines.Add(stripline);
                    chartSmoke.Series["Smoke Level"].Points.Clear();
                    i = 1;
                    foreach (object chk in chkSmoke.Items) {
                        if (chkSmoke.GetItemChecked(chkSmoke.Items.IndexOf(chk))) {
                            chartSmoke.Series["Smoke Level"].Points.AddXY(chkSmoke.Items.IndexOf(chk) + 1, yy);
                            i++;
                        }
                    }
                }
            }
            catch { }
        }

        public void EnableTDKRows() {
#if DEBUG
#else
            var ii = 0;
            foreach (var chk in _checkBoxes) {
                chk.Enabled = _TDKconnection[ii];
                ii++;
            }
            ii = 0;
            //foreach (var chk in _tempSensors) {
            //    chk.Enabled = _TDKconnection[ii];
            //    ii++;
            //}
            //ii = 0;
            //foreach (var chk in _setCurrents) {
            //    chk.Enabled = _TDKconnection[ii];
            //    ii++;
            //}
            ii = 0;
            foreach (var chk in _loadButtons) {
                chk.Enabled = _TDKconnection[ii];
                ii++;
            }
            //ii = 0;
            //foreach (var chk in _voc) {
            //    chk.Enabled = _TDKconnection[ii];
            //    ii++;
            //}
            //ii = 0;
            //foreach (var chk in _numCells) {
            //    chk.Enabled = _TDKconnection[ii];
            //    ii++;
            //}
#endif
        }


        public static void Info(object m, string module = "Server") {
            U.Logger.WriteLine($@"[{DateTime.Now:G}]:[{module}] {m}");
        }

        private void BtnStart_Click(object sender, EventArgs e) {
            var samples = new List<string>();
            foreach (var s in _samples) samples.Add(s.Text);
            if (samples.GroupBy(n => n).Any(c => c.Count() > 1)) {
                U.Logger.WriteLine($"Duplicate Sample Names Choosen!");
                MessageBox.Show($"Duplicate Sample Names Choosen!");
                return;
            }
            if (!_TDKconnection.Any(b => b == true)) {
                U.Logger.WriteLine($"TDK has no connections!");
                MessageBox.Show($"TDK has no connections!");
                return;
            }
            if (!_arduino.Connected) {
                U.Logger.WriteLine($"Arduino not connected!");
                MessageBox.Show($"Arduino not connected!");
                return;
            }

            CheckPorts();
            var startargs = new StartCyclingArgs(_TDKS.Where(t => t.Cycling == true).ToList(),
                Double.Parse(txtBiasOn.Text), Double.Parse(txtBiasOff.Text),
                Double.Parse(textBoxOverVoltage.Text));

            var start = new CoreCommand {
                Type = U.CmdType.StartCycling,
                StartArgs = startargs
            };
            NewCoreCommand(this, start);

            // Disable GUI buttons from the GUI thread 
            foreach (var chk in _checkBoxes) {
                chk.Enabled = false;
            }
            foreach (var temp in _tempSensors) {
                temp.Enabled = false;
            }
            foreach (var curr in _setCurrents) {
                curr.Enabled = false;
            }
            foreach (var load in _loadButtons) {
                load.Enabled = false;
            }
            foreach (var neww in _voc) {
                neww.Enabled = false;
            }
            foreach (var neww in _numCells) {
                neww.Enabled = false;
            }
            chkTemp.Enabled = false;
            chkSmoke.Enabled = false;
            btnStart.Enabled = false;
            btnCheckConnection.Enabled = false;
            btnLoadSamples.Enabled = false;
            btnClearSamples.Enabled = false;
            buttonClearAlarms.Enabled = false;

            // save GUI inputs to default settings
            Properties.Settings.Default.Operator = txtOperator.Text;
            Properties.Settings.Default.BiasON = txtBiasOn.Text;
            Properties.Settings.Default.BiasOFF = txtBiasOff.Text;
            Properties.Settings.Default.BiasONTempSet = txtCurrOnTempSet.Text;
            Properties.Settings.Default.BiasOFFTempSet = txtCurrOffTempSet.Text;
            Properties.Settings.Default.OverTempSet = txtOverTempSet.Text;
            Properties.Settings.Default.OverSmokeSet = txtSmokeOverSet.Text;
            Properties.Settings.Default.PauseFanTime = txtPauseFans.Text;
            var iii = 0;
            foreach (var chk in _checkBoxes) {
                Properties.Settings.Default.CheckBoxes[iii] = chk.Checked;
                iii++;
            }
            var ii = 0;
            foreach (object chk in chkTemp.Items) {
                Properties.Settings.Default.ActiveTemps[ii] = chkTemp.GetItemChecked(chkTemp.Items.IndexOf(chk));
                ii++;
            }
            ii = 0;
            foreach (object chk in chkSmoke.Items) {
                Properties.Settings.Default.ActiveSmokes[ii] = chkSmoke.GetItemChecked(chkSmoke.Items.IndexOf(chk));
                ii++;
            }
            Properties.Settings.Default.Save();

        }

        /// <summary>
        /// Loop through 12 TDK port check boxes. If checked, then initialize TDK object from form data
        /// </summary>
        private void CheckPorts() {
            for (var i = 0; i < 12; i++) {
                if (_checkBoxes[i].Checked) {
                    _TDKS.Where(t => t.Port == i + 1).FirstOrDefault().Cycling = true;
                    _TDKS.Where(t => t.Port == i + 1).FirstOrDefault().SetCurrent = _setCurrents[i].Text;
                    _TDKS.Where(t => t.Port == i + 1).FirstOrDefault().TempSensor = _tempSensors[i].Text;
                    _TDKS.Where(t => t.Port == i + 1).FirstOrDefault().SampleName = _samples[i].Text;
                    _TDKS.Where(t => t.Port == i + 1).FirstOrDefault().Voc = _voc[i].Text;
                    _TDKS.Where(t => t.Port == i + 1).FirstOrDefault().NumCells = _numCells[i].Text;
                    _TDKS.Where(t => t.Port == i + 1).FirstOrDefault().CycleCount = int.Parse(_cycleLabels[i].Text);
                }
            }
        }

        private void UpdateSmokeRefs(List<double> smokes) {
            var i = 0;
            foreach (var s in smokes) {
                if (s < _refSmokes[i]) {
                    _refSmokes[i] = s; // update reference if volt less than reference
                }
                _smokeLevel[i] = s - _refSmokes[i]; // update smoke level
                i++;
            }
        }

        private bool CheckSmokeOver(List<double> smokes) {
            // filter out smokes that are not checked
            var filtered = new List<double> { };
            var i = 0;
            foreach (object chk in chkSmoke.Items) {
                if (chkSmoke.GetItemChecked(chkSmoke.Items.IndexOf(chk))) {
                    filtered.Add(_smokeLevel[i]);
                }
                i++;
            }

            // check if smoke over set point
            foreach (var s in filtered) {
                if (s > double.Parse(txtSmokeOverSet.Text)) {
                    Console.WriteLine($"OVERSMOKE Value: {s}");
                    return true;
                }
            }
            return false;
        }

        private string GetBinary(bool value) {
            return (value == true ? "1" : "0");
        }

        private void Form_Closing(object s, FormClosingEventArgs e) {
            if (_TDKWorker.IsBusy) {
                MessageBox.Show("Please Stop Cycling before Closing Form! ");
                e.Cancel = true;
            }
            U.Logger.SaveLog();
            Properties.Settings.Default.Operator = txtOperator.Text;
            Properties.Settings.Default.BiasON = txtBiasOn.Text;
            Properties.Settings.Default.BiasOFF = txtBiasOff.Text;
            Properties.Settings.Default.BiasONTempSet = txtCurrOnTempSet.Text;
            Properties.Settings.Default.BiasOFFTempSet = txtCurrOffTempSet.Text;
            Properties.Settings.Default.OverTempSet = txtOverTempSet.Text;
            Properties.Settings.Default.OverSmokeSet = txtSmokeOverSet.Text;
            Properties.Settings.Default.PauseFanTime = txtPauseFans.Text;
            var iii = 0;
            foreach (var chk in _checkBoxes) {
                Properties.Settings.Default.CheckBoxes[iii] = chk.Checked;
                iii++;
            }
            var ii = 0;
            foreach (object chk in chkTemp.Items) {
                Properties.Settings.Default.ActiveTemps[ii] = chkTemp.GetItemChecked(chkTemp.Items.IndexOf(chk));
                ii++;
            }
            ii = 0;
            foreach (object chk in chkSmoke.Items) {
                Properties.Settings.Default.ActiveSmokes[ii] = chkSmoke.GetItemChecked(chkSmoke.Items.IndexOf(chk));
                ii++;
            }
            Properties.Settings.Default.Save();

        }

        private void ButtonCheckConnection_Click(object sender, EventArgs e) {
            NewCoreCommand(this, new CoreCommand { Type = U.CmdType.CheckConnection });
            btnCheckConnection.Enabled = false;
            btnStart.Enabled = false;
        }

        public List<string> CheckConnection() {
            var ser = new SerialPort();
            var connectLabels = new List<string>();
            var ports = SerialPort.GetPortNames().ToList();
            U.Logger.WriteLine($"Port Names:");
            ports.ForEach(U.Logger.WriteLine);
            // ping each port and see if we get the TDK response
            foreach (var port in ports) {
                try {
                    ser.BaudRate = U.TDKBaudRate;
                    ser.PortName = port;
                    ser.NewLine = "\r";
                    ser.ReadTimeout = 200;
                    ser.Open();

                    ser.Write("ADR " + "01" + "\r\n");
                    if (ser.ReadLine() == "OK") {
                        ser.DiscardOutBuffer();
                        ser.DiscardInBuffer();
                        break;
                    }
                }
                catch (Exception exc) {
                    U.Logger.WriteLine($"{exc}");
                    ser.Close();
                }
            }

            // loop through each TDK, wait for response if connected
            for (var i = 1; i < 13; i++) {
                try {
                    ser.Write("ADR " + $"0{i.ToString()}" + "\r\n");
                    // TDK address is connected
                    if (ser.ReadLine() == "OK") {
                        connectLabels.Add("Connected");
                        ser.DiscardOutBuffer();
                        ser.DiscardInBuffer();
                        _TDKconnection[i - 1] = true;
                        continue;
                    }
                }
                catch { } // timed out
                connectLabels.Add("Not Connected");
                _TDKconnection[i - 1] = false;
            }
            ser.Close();
            return connectLabels;
        }

        private void BtnStop_Click(object sender, EventArgs e) {
            NewCoreCommand(this, new CoreCommand { Type = U.CmdType.StopCycling });
        }


        private void ButtonRecipe_Click(object sender, EventArgs e) {
            // use Btn sender to parse through control lists
            string txt = ((Button)sender).Name;
            int index;
            if (txt.Length == 8) {
                index = int.Parse(txt.Substring(txt.Length - 1)) - 1;
            }
            else {
                index = int.Parse(txt.Substring(txt.Length - 2)) - 1;
            }

            var form = new RecipeEditor<CCRecipe>(BQConn, new CCRecipe(), _samples[index].Text);
            form.ShowDialog();
            _ccRecipe = form.CurrentRecipe;

            if (_ccRecipe.SampleName != null) {
                Properties.Settings.Default.Samples[index] = _ccRecipe.SampleName;
                Properties.Settings.Default.Save();
                _samples[index].Text = _ccRecipe.SampleName;
                _cycleLabels[index].Text = _ccRecipe.CycleNumber.ToString();
                _numCells[index].Text = _ccRecipe.NumCells.ToString();
                _voc[index].Text = _ccRecipe.CellVoc.ToString();
                _tempSensors[index].Text = _ccRecipe.TempSensor.ToString();
                _setCurrents[index].Text = _ccRecipe.SetCurrent.ToString();
            }


        }


        
        private void BtnLoadSamples_Click(object sender, EventArgs e) {
            var i = 0;
            foreach (var s in Properties.Settings.Default.Samples) {
                try {
                    if (s == null) {
                        i++;
                        continue;
                    }
                    Console.WriteLine($"Loading Recipes...");
                    var recipe = BQConn.GetCurrentRecipe(s);
                    _samples[i].Text = recipe.SampleName;
                    _cycleLabels[i].Text = recipe.CycleNumber.ToString();
                    _numCells[i].Text = recipe.NumCells.ToString();
                    _voc[i].Text = recipe.CellVoc.ToString();
                    _tempSensors[i].Text = recipe.TempSensor.ToString();
                    _setCurrents[i].Text = recipe.SetCurrent.ToString();
                }
                catch (Exception exc) {
                    U.Logger.WriteLine(exc.ToString());
                }
                i++;
            }
        }

        private void BtnClearSamples_Click(object sender, EventArgs e) {
            Properties.Settings.Default.Samples = new List<string> { null, null, null, null, null, null, null, null, null, null, null, null };
            Properties.Settings.Default.Save();
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e) {
            yy++;
            _commWorker.ReportProgress(6);
        }

        private void ButtonClearAlarms_Click_1(object sender, EventArgs e) {
            SMOKEALARM = false;
        }

        private void CheckBoxPrintPacket_CheckedChanged(object sender, EventArgs e) {
            _arduinoWorker.ReportProgress(1);
        }

        private void ButtonSaveLogs_Click(object sender, EventArgs e) {
            U.Logger.SaveLog();
#if DEBUG
            for (var i = 0; i < 5; i++) {
                TestDataEvent?.Invoke(this, new DataQueueObject(DataQueueObject.DataType.CycleData,
                new CCDataPoint(5,
                66666666.253, // epoch
                1231212313.35, // total time (hrs)
                121313123.3434343, // time into current cycle
                false, "Testing", 1, 1, 1,
                1, 1, 10.2, -99.99,
                new List<double> { 5, 5,5,5,5,5,5,5,5,5,5,5,5,5,5,5 }, new List<double> { 5, 5, 5, 5, 5, 5, 5, 5 }, new List<double> { 5, 5, 5, 5, 5, 5, 5, 5 })));
            }

#endif
        }

    }
}
