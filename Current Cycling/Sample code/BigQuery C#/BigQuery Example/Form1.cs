using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.BigQuery.V2;

namespace BigQuery_Example
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnAddData_Click(object sender, EventArgs e)
        {
            //var credentials = GoogleCredential.FromFile(jsonPath);
            //var client = BigQueryClient.Create(projectId, credentials);

            //Creates a connection to the database
            BigQueryClient client = BigQueryClient.Create("booming-pride-278623");


            // Create the dataset if it doesn't exist.
            BigQueryDataset dataset = client.GetOrCreateDataset("TestBQ");

            // Create the table if it doesn't exist.
            BigQueryTable table = dataset.GetOrCreateTable("Test_Table_3", new TableSchemaBuilder
                        {
                            { "LogTime", BigQueryDbType.Timestamp },
                            { "CycleNumber", BigQueryDbType.Int64 },
                            { "Epoch", BigQueryDbType.Int64 },
                            { "TotalTime", BigQueryDbType.Int64 },
                            { "TimeIntoCycle", BigQueryDbType.Int64 },
                            { "CurrentBias", BigQueryDbType.Bool },
                            { "Current", BigQueryDbType.Float64 },
                            { "Voltage", BigQueryDbType.Float64 },
                            { "Add", BigQueryDbType.Float64 },

                        }.Build());

            // Insert a single row. There are many other ways of inserting
            // data into a table.
            var now = DateTime.UtcNow;
            var dt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond);
            //string currentdatetime = DateTime.Now.ToUniversalTime().ToString("yyyyMMdd[HH:mm:ss]");


            for (var i = 0; i < 200; i++) {
                now = DateTime.UtcNow;
                dt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond);
                table.InsertRow(new BigQueryInsertRow
                        {
                            { "LogTime", dt },
                            { "CycleNumber", 0 + i },
                            { "Epoch", 1000 + i },
                            { "TotalTime", 500 +i },
                            { "TimeIntoCycle", 111 + i },
                            { "CurrentBias", i % 2 == 0 ? false: true },
                            { "Current", 2 + 2*i },
                            { "Voltage", 2 + 4*i },
                            { "Add", 55569 },
                        });
            }
            

        }

        public class CCDataPoint {
            //public ObjectId Id { get; set; }
            public DateTime LogTime { get; set; }
            public int CycleNumber { get; set; }
            public double Epoch { get; set; }
            public double TotalTime { get; set; }
            public double TimeIntoCycle { get; set; }
            public bool CurrentBias { get; set; }
            public string SampleName { get; set; }
            public double Current { get; set; }
            public double Voltage { get; set; }
            public int NumCells { get; set; }
            public double CellVoc { get; set; }
            public int TempSensor { get; set; }
            public double SetCurrent { get; set; }
            public double EstimatedRs { get; set; }
            public List<double> Temps { get; set; }
            public List<double> SmokeVoltage { get; set; }
            public List<double> SmokeLevel { get; set; }


            public CCDataPoint(int cycle, double ep, double totaltime, double timeinto, bool bias, string sample,
                double curr, double voltage, int numcells, double cellVoc, int tempsensor, double setcurrent,
                double rs, List<double> temps, List<double> smokevolts, List<double> smokelevel) {
                //Id = ObjectId.GenerateNewId();
                //LogTime = DateTime.Now;
                CycleNumber = cycle;
                Epoch = ep;
                TotalTime = totaltime;
                TimeIntoCycle = timeinto;
                CurrentBias = bias;
                SampleName = sample;
                Current = curr;
                Voltage = voltage;
                NumCells = numcells;
                CellVoc = cellVoc;
                TempSensor = tempsensor;
                SetCurrent = setcurrent;
                EstimatedRs = rs;
                Temps = temps;
                SmokeVoltage = smokevolts;
                SmokeLevel = smokelevel;

            }
        }

    }
}
