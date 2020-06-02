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
        private BigQueryClient client;
        private BigQueryDataset dataset;
        public const string SampleName = "SampleName";


        public frmMain()
        {
            InitializeComponent();
            //Creates a connection to the database
            client = BigQueryClient.Create("booming-pride-278623");


            // Create the dataset if it doesn't exist.
            dataset = client.GetOrCreateDataset("TestBQ");

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnAddData_Click(object sender, EventArgs e)
        {
            //var credentials = GoogleCredential.FromFile(jsonPath);
            //var client = BigQueryClient.Create(projectId, credentials);

            

            // Create the table if it doesn't exist.
            BigQueryTable table = dataset.GetOrCreateTable("Test_Data_Recipe", new TableSchemaBuilder
            {
                { "LogTime", BigQueryDbType.Timestamp },
                { "SampleName", BigQueryDbType.String },
                { "CycleNumber", BigQueryDbType.Int64 },
                { "Epoch", BigQueryDbType.Int64 },
                { "TotalTime", BigQueryDbType.Int64 },
                { "TimeIntoCycle", BigQueryDbType.Int64 },
                { "CurrentBias", BigQueryDbType.Bool },
                { "Current", BigQueryDbType.Float64 },
                { "Voltage", BigQueryDbType.Float64 },
                { "EstimatedRs", BigQueryDbType.Float64 },
                { "Temps", BigQueryDbType.Float64, BigQueryFieldMode.Repeated },
                { "SmokeVoltage", BigQueryDbType.Float64, BigQueryFieldMode.Repeated  },
                { "SmokeLevel", BigQueryDbType.Float64, BigQueryFieldMode.Repeated  },
                { "NumCells", BigQueryDbType.Int64 },
                { "CellVoc", BigQueryDbType.Float64 },
                { "TempSensor", BigQueryDbType.Int64 },
                { "SetCurrent", BigQueryDbType.Float64 },

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
                    { "SampleName", "Test1" },
                    { "CycleNumber", 11 },
                    { "Epoch", 1 },
                    { "TotalTime", 2 },
                    { "TimeIntoCycle", 3 },
                    { "CurrentBias", false },
                    { "Current", 4 },
                    { "Voltage", 5 },
                    { "EstimatedRs", 6 },
                    { "Temps", new List<double>{ 60,60 } },
                    { "SmokeVoltage", new List<double>{ 60,60 } },
                    { "SmokeLevel", new List<double>{ 60,60 } },
                    { "NumCells", 8 },
                    { "CellVoc", 9 },
                    { "TempSensor", 9 },
                    { "SetCurrent", 99 },
                });
            }
        }

        private void BtnQueryClick(object s, EventArgs e) {
            var table = dataset.GetTable("Test_Data_Recipe");
            var sample = "Test1";
            var sql = $"SELECT * FROM {table} WHERE SampleName = \"{sample}\" ORDER BY LogTime DESC LIMIT 1";

            var results = client.ExecuteQuery(sql, null);
            var row = results.FirstOrDefault();
            var lst = row["Temps"] as double[];

            CCDataPoint pt = new CCDataPoint()
            {
                CycleNumber = int.Parse(row["CycleNumber"].ToString()),
                Epoch = int.Parse(row["Epoch"].ToString()),
                TotalTime = int.Parse(row["TotalTime"].ToString()),
                TimeIntoCycle = int.Parse(row["TimeIntoCycle"].ToString()),
                CurrentBias = bool.Parse(row["CurrentBias"].ToString()),
                SampleName = row["SampleName"].ToString(),
                Current = double.Parse(row["Current"].ToString()),
                Voltage = double.Parse(row["Voltage"].ToString()),
                NumCells = int.Parse(row["NumCells"].ToString()),
                CellVoc = double.Parse(row["CellVoc"].ToString()),
                TempSensor = int.Parse(row["TempSensor"].ToString()),
                SetCurrent = double.Parse(row["SetCurrent"].ToString()),
                EstimatedRs = double.Parse(row["EstimatedRs"].ToString()),
                Temps = (row["Temps"] as double[]).ToList(),
                SmokeVoltage = (row["SmokeVoltage"] as double[]).ToList(),
                SmokeLevel = (row["SmokeLevel"] as double[]).ToList()
            };

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

            public CCDataPoint() {
            }

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
