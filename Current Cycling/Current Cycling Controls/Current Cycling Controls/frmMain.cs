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


namespace Current_Cycling_Controls
{
    public delegate void HeartBeatUpdate(object sender, TransmitPacket t);
    public partial class frmMain : Form
    {
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
        private string Cycling;

        private DateTime _cycleTimer = DateTime.Now;
        private TransmitPacket _heartBeatPacket;
        public frmMain()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(Form_Closing);

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
            _tempLabels = new List<Label> { labelTemp1, labelTemp2,
            labelTemp3,labelTemp4,labelTemp5,labelTemp6,
            labelTemp7,labelTemp8,labelTemp9,labelTemp10,
            labelTemp11,labelTemp12, labelTemp13, labelTemp14, labelTemp15, labelTemp16};
            _smokeLabels = new List<Label> { labelSmoke1, labelSmoke2,
            labelSmoke3,labelSmoke4,labelSmoke5,labelSmoke6,
            labelSmoke7,labelSmoke8};
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
            _newButtons = new List<Button> { btnNew1, btnNew2, btnNew3 , btnNew4 ,
            btnNew5,btnNew6,btnNew7,btnNew8,btnNew9,btnNew10,btnNew11,btnNew12};
            _voc = new List<TextBox> { txtVoc1, txtVoc2 , txtVoc3 , txtVoc4 , txtVoc5 ,
            txtVoc6,txtVoc7,txtVoc8,txtVoc9,txtVoc10,txtVoc11,txtVoc12};
            _numCells = new List<TextBox> { txtNumCells1, txtNumCells2 , txtNumCells3 ,
            txtNumCells4,txtNumCells5,txtNumCells6,txtNumCells7,txtNumCells8,txtNumCells9
            ,txtNumCells10,txtNumCells11,txtNumCells12};

            // reload default settings to GUI
            txtDirectory.Text = Properties.Settings.Default.DataFolder;
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

            // set TDK rows to disabled until the connnection is good
            CheckTDKRows();

            // initialize TDK objects
            _TDKS = new List<TDK> { };
            for (int i = 1; i <13; i++) {
                _TDKS.Add(new TDK("0" + i, i));
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

            while (!_arduino.Connected)
            {

            }
            _connectionWorker.RunWorkerAsync();
            Console.WriteLine($"Checking TDK connections");
            btnStart.Enabled = false;
            btnCheckConnection.Enabled = false;
        }

        private void RunCurrentCycling (object s, DoWorkEventArgs e) {
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
                t.SetCurrent = null;
            }
        }

        private void RunArduinoLoop(object s, DoWorkEventArgs e) {
            _arduino.StartArduinoMachine();
        }

        public void UpdateHeartBeat(object sender, ProgressChangedEventArgs e) {
            _arduino.UpdateTransmit(_heartBeatPacket);
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
            //Info(c);
            switch (c.Type) {
                case U.CmdType.None:
                    break;
                case U.CmdType.StartCycling:
                    Console.WriteLine($"Starting TDK Worker thread");
                    NewCoreCommand(this, new CoreCommand { Type = U.CmdType.UpdateHeartBeatPacket });
                    
                    _TDKWorker.RunWorkerAsync(c.StartArgs);
                    // communicate with TDKs with the StartInspectionProperties
                    break;
                case U.CmdType.UpdateUI:
                    _commWorker.ReportProgress(5, c.StartArgs);
                    break;
                case U.CmdType.StopCycling:
                    _cycling.STOP = true;
                    break;
                case U.CmdType.CleanGUI:
                    _commWorker.ReportProgress(1);
                    break;
                case U.CmdType.RecievedPacket:
                    // update GUI with temp/smoke/alarms
                    _commWorker.ReportProgress(2, c.ArduinoArgs);

                    // if cycling is running then update alarms if needed
                    var packet = _arduino._recievedPacket;
                    if (_TDKWorker.IsBusy) {
                        _cycling.SMOKEALARM = packet.SmokeAlarm;
                        _cycling.TEMPALARM = packet.TempAlarm;
                        _cycling.STOP = packet.EMSSTOP;
                        _cycling._temps = new List<double>(packet.TempList);
                    }

                    if (packet.SmokeAlarm || packet.TempAlarm || packet.EMSSTOP) {
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
                    _voltageLabels[args.Port-1].Text = args.Volt;
                    _currentLabels[args.Port-1].Text = args.Current;
                    _cycleLabels[args.Port-1].Text = args.Cycle; 

                    var ts = (args.CycleTime - DateTime.Now);
                    labelCount.Text = $@"{ts.Minutes:D2}:{ts.Seconds:D2}";
                    return;
                }
                // re-enable GUI buttons
                else if (e.ProgressPercentage == 1) {
                    CheckTDKRows();
                    //var ii = 0;
                    //foreach (var chk in _checkBoxes) {
                    //    chk.Enabled = _TDKconnection[ii];
                    //    ii++;
                    //}
                    //foreach (var temp in _tempSensors) {
                    //    temp.Enabled = true;
                    //}
                    //foreach (var curr in _setCurrents) {
                    //    curr.Enabled = true;
                    //}
                    //foreach (var load in _loadButtons) {
                    //    load.Enabled = true;
                    //}
                    //foreach (var neww in _newButtons) {
                    //    neww.Enabled = true;
                    //}
                    //foreach (var neww in _voc) {
                    //    neww.Enabled = true;
                    //}
                    //foreach (var neww in _numCells) {
                    //    neww.Enabled = true;
                    //}
                    btnStart.Enabled = true;
                    chkTemp.Enabled = true;
                    chkSmoke.Enabled = true;
                    button1.Enabled = true;
                    btnCheckConnection.Enabled = true;
                    return;
                }
                // update temp/smoke/alarm readings
                else if (e.ProgressPercentage == 2) {
                    var ardArgs = _arduino._recievedPacket;
                    var i = 0;
                    foreach (var lb in _tempLabels) {
                        lb.Text = ardArgs.TempList[i].ToString("F1"); 
                        i++;
                    }
                    i = 0;
                    foreach (var lb in _smokeLabels) {
                        lb.Text = ardArgs.SmokeList[i].ToString("F1");
                        i++;
                    }

                    labelTempAlarm.BackColor = ardArgs.TempAlarm ? Color.Red : Color.Empty;
                    labelSmokeAlarm.BackColor = ardArgs.SmokeAlarm ? Color.Red : Color.Empty;
                    labelEMSStop.BackColor = ardArgs.EMSSTOP ? Color.Red : Color.Empty;

                }
                // send event to arduino thread to update serial transmit packet
                else if (e.ProgressPercentage == 3){
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
                    CheckTDKRows();
                    btnCheckConnection.Enabled = true;
                    btnStart.Enabled = true;
                }
            }
            catch { }
        }

        public void CheckTDKRows() {
            var ii = 0;
            foreach (var chk in _checkBoxes) {
                chk.Enabled = _TDKconnection[ii];
                ii++;
            }
            ii = 0;
            foreach (var chk in _tempSensors) {
                chk.Enabled = _TDKconnection[ii];
                ii++;
            }
            ii = 0;
            foreach (var chk in _setCurrents) {
                chk.Enabled = _TDKconnection[ii];
                ii++;
            }
            ii = 0;
            foreach (var chk in _loadButtons) {
                chk.Enabled = _TDKconnection[ii];
                ii++;
            }
            ii = 0;
            foreach (var chk in _newButtons) {
                chk.Enabled = _TDKconnection[ii];
                ii++;
            }
            ii = 0;
            foreach (var chk in _voc) {
                chk.Enabled = _TDKconnection[ii];
                ii++;
            }
            ii = 0;
            foreach (var chk in _numCells) {
                chk.Enabled = _TDKconnection[ii];
                ii++;
            }
        }


        public static void Info(object m, string module = "Server") {
            Console.WriteLine($@"[{DateTime.Now:G}]:[{module}] {m}");
        }

        private void BtnStart_Click(object sender, EventArgs e) {
            if (!_TDKconnection.Any(b => b == true)) {
                Console.WriteLine($"TDK has no connections!");
                MessageBox.Show($"TDK has no connections!");
                return;
            }
            if (!_arduino.Connected) {
                Console.WriteLine($"Arduino not connected!");
                MessageBox.Show($"Arduino not connected!");
                return;
            }
            CheckPorts();
            var startargs = new StartCyclingArgs(_TDKS.Where(t => t.SetCurrent != null).ToList(), 
                Double.Parse(txtBiasOn.Text), Double.Parse(txtBiasOff.Text), txtDirectory.Text);

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
            foreach (var neww in _newButtons) {
                neww.Enabled = false;
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
            button1.Enabled = false;

            // save GUI inputs to default settings
            Properties.Settings.Default.DataFolder = txtDirectory.Text;
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
        /// Loop through 12 TDK port check boxes. If checked, then initialize TDK object
        /// </summary>
        private void CheckPorts() {
            for (var i = 0; i < 12; i++) {
                bool checkked = _checkBoxes[i].Checked;
                if (checkked) {
                    _TDKS.Where(t => t.Port == i+1).FirstOrDefault().SetCurrent = _setCurrents[i].Text;
                    _TDKS.Where(t => t.Port == i+1).FirstOrDefault().TempSensor = _tempSensors[i].Text;
                    _TDKS.Where(t => t.Port == i+1).FirstOrDefault().SampleName = _samples[i].Text;
                    _TDKS.Where(t => t.Port == i+1).FirstOrDefault().Voc = _voc[i].Text;
                    _TDKS.Where(t => t.Port == i+1).FirstOrDefault().NumCells = _numCells[i].Text;
                    _TDKS.Where(t => t.Port == i+1).FirstOrDefault().CycleCount = int.Parse(_cycleLabels[i].Text);
                }
            }
        }

        private string GetBinary(bool value) {
            return (value == true ? "1" : "0");
        }

        private void Form_Closing(object s, FormClosingEventArgs e) {
            if (_TDKWorker.IsBusy) {
                MessageBox.Show("Please Stop Cycling before Closing Form! ");
                e.Cancel = true;
            }
        }

        private void ButtonCheckConnection_Click(object sender, EventArgs e) {
            NewCoreCommand(this, new CoreCommand { Type = U.CmdType.CheckConnection });
            btnCheckConnection.Enabled = false;
            btnStart.Enabled = false;
        }

        public List<string> CheckConnection() {
            var ser = new SerialPort();
            var connectLabels = new List<string>();
            string[] ports = SerialPort.GetPortNames();
            // ping each port and see if we get the TDK response
            foreach (var port in ports) { 
                try {
                    ser.BaudRate = U.BaudRate;
                    ser.PortName = port;
                    ser.NewLine = "\r";
                    ser.ReadTimeout = 25;
                    ser.Open();

                    ser.Write("ADR " + "01" + "\r\n");
                    if (ser.ReadLine() == "OK") {
                        ser.DiscardOutBuffer();
                        ser.DiscardInBuffer();
                        break;
                    }
                }
                catch { }
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
            }
            ser.Close();
            return connectLabels;
        }


        private void ChkbxPort1_CheckedChanged(object sender, EventArgs e) {
            //if (chkbxPort1.Checked) {
            //    btnLoad1.Enabled = true;
            //    btnNew1.Enabled = true;
            //}
        }

        private void BtnStop_Click(object sender, EventArgs e) {
            NewCoreCommand(this, new CoreCommand{ Type = U.CmdType.StopCycling });
        }

        private void ButtonDataFolder_Click(object sender, EventArgs e) {
            var folderPath = new FolderBrowserDialog();
            if (folderPath.ShowDialog() == DialogResult.Cancel) return;
            Properties.Settings.Default.DataFolder = folderPath.SelectedPath;
            txtDirectory.Text = folderPath.SelectedPath;
            Properties.Settings.Default.Save();
        }

        // TODO: Real Devs would create a control class 
        private void BtnNew_Click(object sender, EventArgs e) {
            // create new file upload dialog and user choose folder then put in sample name.txt
            var saveFile = new SaveFileDialog() { InitialDirectory = Properties.Settings.Default.DataFolder };
            if (saveFile.ShowDialog() == DialogResult.Cancel) return;
            if (File.Exists(saveFile.FileName + ".txt")) {
                Console.WriteLine($"File already exists!");
                return;
            }
            using (var writer = new StreamWriter(saveFile.FileName + ".txt", true)) {
                writer.WriteLine(U.SampleTxtHeader);
            }

            // use btn props to parse through control lists
            string txt = ((Button)sender).Name;
            int index;
            if (txt.Length == 7) index = int.Parse(txt.Substring(txt.Length - 1)) - 1;
            else index = int.Parse(txt.Substring(txt.Length - 2)) - 1;
            _samples[index].Text = Path.GetFileNameWithoutExtension(saveFile.FileName);
            Properties.Settings.Default.DataFolder = Directory.GetParent(saveFile.FileName).FullName;
            Properties.Settings.Default.Save();
            txtDirectory.Text = Directory.GetParent(saveFile.FileName).FullName;
        }

        private void BtnLoad_Click(object sender, EventArgs e) {
            // loads the file and reads the last readline and updates the GUI with values (cycle, voc, set current etc)
            var loadFile = new OpenFileDialog() { InitialDirectory = Properties.Settings.Default.DataFolder };
            if (loadFile.ShowDialog() == DialogResult.Cancel) return;
            Properties.Settings.Default.DataFolder = Directory.GetParent(loadFile.FileName).FullName;
            Properties.Settings.Default.Save();
            txtDirectory.Text = Directory.GetParent(loadFile.FileName).FullName;
            var last = File.ReadLines(loadFile.FileName).Last();
            var values = last.Split(',').Select(sValue => sValue.Trim()).ToList();

            // use btn props to parse through control lists
            string txt = ((Button)sender).Name;
            int index = 0;
            if (txt.Length == 8) {
                index = int.Parse(txt.Substring(txt.Length - 1)) - 1;
            }
            else {
                index = int.Parse(txt.Substring(txt.Length - 2)) - 1;
            }

            _samples[index].Text = Path.GetFileNameWithoutExtension(loadFile.FileName);
            if (File.ReadLines(loadFile.FileName).Count() < 2) {
                _cycleLabels[index].Text = "0";
                _numCells[index].Text = "22";
                _voc[index].Text = "0.655";
                _tempSensors[index].Text = (index+1).ToString();
                _setCurrents[index].Text = "0";
                return;
            }
            _cycleLabels[index].Text = values[0];
            _numCells[index].Text = values[8];
            _voc[index].Text = values[9];
            _tempSensors[index].Text = values[10];
            _setCurrents[index].Text = values[11];

        }

    }
}
