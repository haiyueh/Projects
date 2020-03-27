using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chart_Control_Example
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            chtMain.Series["Voltage by Cycle"].Points.AddXY(1, 3.4);
            chtMain.Series["Voltage by Cycle"].Points.AddXY(2, 4.4);
            chtMain.Series["Voltage by Cycle"].Points.AddXY(3, 1.4);
            chtMain.Series["Voltage by Cycle"].Points.AddXY(4, 3.4);
            chtMain.Series["Voltage by Cycle"].Points.AddXY(5, 5.4);
            chtMain.Series["Voltage by Cycle"].Points.AddXY(6, 7.4);
            chtMain.Series["Voltage by Cycle"].Points.AddXY(7, 3.9);
            chtMain.Series["Voltage by Cycle"].Points.AddXY(8, 8.4);
            chtMain.SaveImage("C:\\temp\\whohoo.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        private void chtMain_Click(object sender, EventArgs e)
        {

        }
    }
}
