using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Media.Media3D;

namespace Chart_Control_Example
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;

        private void button1_Click(object sender, EventArgs e)
        {
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
            var vals = new List<double> { 1.3, 5.5, 5, 6, 7, 8, 5, 4, 10, 11.5, 7 };
            var cycles = 5;
            var lin = Linespace(1, cycles, vals.Count-2);
            var ii = 0;
            foreach (var val in vals) { 
                chart1.Series["Volts"].Points.AddXY(lin[ii], val);
                ii++;
            }
            chart1.Invalidate();

            //for (var i = 0; i < pts; i++) {
            //    chtMain.Series["Voltage by Cycle"].Points.AddXY(i + 1, i + pts / cycles);
            //}
            //chtMain.Series["Voltage by Cycle"].Points.AddXY(1, 3.4);
            //chtMain.Series["Voltage by Cycle"].Points.AddXY(2, 4.4);
            //chtMain.Series["Voltage by Cycle"].Points.AddXY(3, 1.4);
            //chtMain.Series["Voltage by Cycle"].Points.AddXY(4, 3.4);
            //chtMain.Series["Voltage by Cycle"].Points.AddXY(5, 5.4);
            //chtMain.Series["Voltage by Cycle"].Points.AddXY(6, 7.4);
            //chtMain.Series["Voltage by Cycle"].Points.AddXY(7, 3.9);
            //chtMain.Series["Voltage by Cycle"].Points.AddXY(8, 8.4);
            try {
                chart1.SaveImage("C:\\Users\\phoge\\Pictures\\charttt.png", System.Drawing.Imaging.ImageFormat.Png);
                chtMain.SaveImage("C:\\Users\\phoge\\Pictures\\charttt2.png", System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception exc) {
                Console.WriteLine($"{exc}");
            }
            
        }

        private List<double> Linespace(double start, double end, int sections) {
            List<double> lst = new List<double>();
            for (var i = 0; i < sections+2; i++) {
                lst.Add(start + ((end - start) / (sections +1)) * i);
            }
            return lst;
        }

        private void chtMain_Click(object sender, EventArgs e)
        {

        }
    }
}
