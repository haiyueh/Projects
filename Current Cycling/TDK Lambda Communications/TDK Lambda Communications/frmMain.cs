using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;


namespace TDK_Lambda_Communications
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnEnumerate_Click(object sender, EventArgs e)
        {
            //Get a list of serial port names.
            string[] ports = SerialPort.GetPortNames();

            //Clears the combo box
            cmbSerial.Items.Clear();

            //Adds each port name to the combo box
            foreach (string port in ports)
            {
                cmbSerial.Items.Add(port);
            }

        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            //Sets the serial port up and then opens the port
            serTDK.BaudRate = 57600;
            serTDK.PortName = cmbSerial.Text;
            serTDK.NewLine = "\r";
            serTDK.ReadTimeout = 1000;
            serTDK.Open();
            
            //Sets the GUI correctly
            btnClose.Enabled = true;
            btnOpen.Enabled = false;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            //Closes the port
            serTDK.Close();

            //Reenables the open button and disables the close button
            btnOpen.Enabled = true;
            btnClose.Enabled = false;
        }

        //Sets the address of the TDK Lambda
        private Boolean setAddress (string strAddress)
        {
            //Sets the address of the power supply
            serTDK.Write("ADR " + strAddress + "\r\n");

            //Reads the response and see if we got an error or not
            if (serTDK.ReadLine() == "OK")
            {
                //Return success
                return true;
            }
            else
            {
                //Returns failure
                return false;
            }
        }



        //Sets the voltage of the TDK Lambda
        private Boolean setVoltage(string strVoltage)
        {
            //Sets the voltage of the power supply
            serTDK.Write("PV " + strVoltage + "\r\n");

            //Reads the response and see if we got an error or not
            if (serTDK.ReadLine() == "OK")
            {
                //Return success
                return true;
            }
            else
            {
                //Returns failure
                return false;
            }
        }

        //Sets the current of the TDK Lambda
        private Boolean setCurrent(string strCurrent)
        {
            //Sets the current limit of the power supply
            serTDK.Write("PC " + strCurrent + "\r\n");

            //Reads the response and see if we got an error or not
            if (serTDK.ReadLine() == "OK")
            {
                //Return success
                return true;
            }
            else
            {
                //Returns failure
                return false;
            }
        }

        //Sets the output state of the TDK Lambda
        private Boolean setOutput(Boolean bolOn)
        {
            //Checks to see if we want to turn it on or off
            if (bolOn == true)
            {
                //Sets the power supply to be on
                serTDK.Write("OUT 1\r\n");
            }
            else
            {
                //Sets the power supply to be off
                serTDK.Write("OUT 0\r\n");
            }

            //Reads the response and see if we got an error or not
            if (serTDK.ReadLine() == "OK")
            {
                //Return success
                return true;
            }
            else
            {
                //Returns failure
                return false;
            }
        }

        //Gets the instantaneous voltage of the power supply
        private string getVoltage()
        {
            //Sets the current limit of the power supply
            serTDK.Write("MV?\r\n");

            //Reads the response and returns it
            return serTDK.ReadLine();     
        }

        //Gets the instantaneous current of the power supply
        private string getCurrent()
        {
            //Sets the current limit of the power supply
            serTDK.Write("MC?\r\n");

            //Reads the response and returns it
            return serTDK.ReadLine();
        }

        private void btnSetAddress_Click(object sender, EventArgs e)
        {
            setAddress(txtAddress.Text);
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

        }

        private void btnSetCurrent_Click(object sender, EventArgs e)
        {
            setCurrent(txtCurrent.Text);

        }

        private void btnSetVoltage_Click(object sender, EventArgs e)
        {
            setVoltage(txtVoltage.Text);
        }

        private void btnOn_Click(object sender, EventArgs e)
        {
            //Turns the power supply on and starts reading voltage and current
            setOutput(true);
            tmrMain.Enabled = true;
        }

        private void btnOff_Click(object sender, EventArgs e)
        {
            //Turns the power supply off and disables voltage/current reading
            setOutput(false);
            tmrMain.Enabled = false;
        }

        private void tmrMain_Tick(object sender, EventArgs e)
        {
            //Reads the voltage and current from the power supply
            lblInstVoltage.Text = "Voltage: " + getVoltage();
            lblInstCurrent.Text = "Current: " + getCurrent();
        }
    }
}
