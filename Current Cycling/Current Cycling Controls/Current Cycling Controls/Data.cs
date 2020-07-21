using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.BigQuery.V2;
using Google.Apis.Bigquery.v2.Data;
using System.IO;

namespace Current_Cycling_Controls {

    public delegate void DataEvent(object sender, DataQueueObject e);
    public class Data {
        private static readonly BigQueryClient _client = BigQueryClient.Create("booming-pride-278623");
        private BigQueryDataset _dataset;
        private BigQueryTable _table;

        private readonly Queue<DataQueueObject> _dataQueue = new Queue<DataQueueObject>();
        private readonly BackgroundWorker _dataWorker = new BackgroundWorker();
        private readonly AutoResetEvent _reset = new AutoResetEvent(false);
        private readonly object _lock = new object();
        private readonly string ComputerName = Environment.MachineName.ToUpper();
        public bool DataError = false;

        public Data() {
            if (!BuildTable()) Console.WriteLine($"Couldn't Connect to Google BQ. Please check connection!");
            _dataWorker.DoWork += DataWorker_DoWork;
            _dataWorker.RunWorkerAsync();
        }

        private void DataWorker_DoWork(object sender, DoWorkEventArgs e) {
            while (true) {
                DataQueueObject d = null;
                lock (_lock) {
                    if (_dataQueue.Count > 0) {
                        d = _dataQueue.Dequeue();
                    }
                }
                if (d == null) {
                    _reset.WaitOne(5000);
                    continue;
                }
                HandleDataEvent(d);
            }
        }

        /// <summary>
        /// Function for creating new documents 
        /// </summary>
        /// <param name="d"></param>
        private void HandleDataEvent(DataQueueObject d) {
            switch (d.Type) {
                case DataQueueObject.DataType.CycleData:
                    if (!(d.Data is CCDataPoint p)) return;
                    SaveCCData(p);
                    break;
            }
        }

        /// <summary>
        /// Data queue event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="d"></param>
        public void QueueData(object sender, DataQueueObject d) {
            lock (_lock) {
                _dataQueue.Enqueue(d);
            }
            _reset.Set();
        }

        /// <summary>
        /// Used for getting SampleNames for comboBox
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <returns></returns>
        public List<string> GetRecipeList<TData>() {
            var lst = new List<string>();
            try {
                string sql = $"SELECT DISTINCT SampleName FROM {_table} LIMIT 500";
                var result = _client.ExecuteQuery(sql, parameters: null);
                foreach (var row in result) {
                    lst.Add(row["SampleName"].ToString());
                }
            }

            catch (TimeoutException) {
            }
            catch (Exception exc) {
                Console.WriteLine(exc);
            }
            if (lst.Contains("Testing")) lst.Remove("Testing");
            return lst;
        }

        public CCRecipe GetCurrentRecipe(string currentSample) {
            var recipe = new CCRecipe();
            try {
                var sql = $"SELECT * FROM {_table} WHERE SampleName = \"{currentSample}\" ORDER BY CycleNumber DESC LIMIT 1";

                var results = _client.ExecuteQuery(sql, null);
                var row = results.FirstOrDefault();
                recipe = new CCRecipe() {
                    SampleName = row["SampleName"].ToString(),
                    NumCells = int.Parse(row["NumCells"].ToString()),
                    CellVoc = double.Parse(row["CellVoc_Volts"].ToString()),
                    TempSensor = int.Parse(row["TempSensorNumber"].ToString()),
                    SetCurrent = double.Parse(row["SetCurrent_Amps"].ToString()),
                    CycleNumber = int.Parse(row["CycleNumber"].ToString()),
                };
            }
            catch (Exception exc) {
                Console.WriteLine(exc);
            }
            return recipe;

        }

        private bool BuildTable() {
            var tableName = ComputerName == "CH-J7TMTZ1" ? "CCChinaTable" : "CCTable";
#if DEBUG
            tableName = "CCChinaTable";
#endif
            try {
                _dataset = _client.GetOrCreateDataset("CCDataset");
                _table = _dataset.GetOrCreateTable(tableName, new TableSchemaBuilder
                    {
                    { "CycleNumber", BigQueryDbType.Int64 },
                    { "LogTime_Timestamp", BigQueryDbType.Timestamp },
                    { "TotalTest_Hours", BigQueryDbType.Float64 },
                    { "MinutesIntoCycle", BigQueryDbType.Float64 },
                    { "CurrentBiasIsOn", BigQueryDbType.String },
                    { "SampleName", BigQueryDbType.String },
                    { "Current_Amps", BigQueryDbType.Float64 },
                    { "Voltage_Volts", BigQueryDbType.Float64 },
                    { "NumCells", BigQueryDbType.Int64 },
                    { "CellVoc_Volts", BigQueryDbType.Float64 },
                    { "TempSensorNumber", BigQueryDbType.Int64 },
                    { "SetCurrent_Amps", BigQueryDbType.Float64 },
                    { "EstimatedRs_mOhms", BigQueryDbType.Float64 },
                    { "Temp1_C", BigQueryDbType.Float64 },
                    { "Temp2_C", BigQueryDbType.Float64 },
                    { "Temp3_C", BigQueryDbType.Float64 },
                    { "Temp4_C", BigQueryDbType.Float64 },
                    { "Temp5_C", BigQueryDbType.Float64 },
                    { "Temp6_C", BigQueryDbType.Float64 },
                    { "Temp7_C", BigQueryDbType.Float64 },
                    { "Temp8_C", BigQueryDbType.Float64 },
                    { "Temp9_C", BigQueryDbType.Float64 },
                    { "Temp10_C", BigQueryDbType.Float64 },
                    { "Temp11_C", BigQueryDbType.Float64 },
                    { "Temp12_C", BigQueryDbType.Float64 },
                    { "Temp13_C", BigQueryDbType.Float64 },
                    { "Temp14_C", BigQueryDbType.Float64 },
                    { "Temp15_C", BigQueryDbType.Float64 },
                    { "Temp16_C", BigQueryDbType.Float64 },
                    { "SmokeLevel1_Volts", BigQueryDbType.Float64 },
                    { "SmokeLevel2_Volts", BigQueryDbType.Float64 },
                    { "SmokeLevel3_Volts", BigQueryDbType.Float64 },
                    { "SmokeLevel4_Volts", BigQueryDbType.Float64 },
                    { "SmokeLevel5_Volts", BigQueryDbType.Float64 },
                    { "SmokeLevel6_Volts", BigQueryDbType.Float64 },
                    { "SmokeLevel7_Volts", BigQueryDbType.Float64 },
                    { "SmokeLevel8_Volts", BigQueryDbType.Float64 },
                    { "SmokeVoltage1_Volts", BigQueryDbType.Float64 },
                    { "SmokeVoltage2_Volts", BigQueryDbType.Float64 },
                    { "SmokeVoltage3_Volts", BigQueryDbType.Float64 },
                    { "SmokeVoltage4_Volts", BigQueryDbType.Float64 },
                    { "SmokeVoltage5_Volts", BigQueryDbType.Float64 },
                    { "SmokeVoltage6_Volts", BigQueryDbType.Float64 },
                    { "SmokeVoltage7_Volts", BigQueryDbType.Float64 },
                    { "SmokeVoltage8_Volts", BigQueryDbType.Float64 },
                }.Build());
                return true;
            }
            catch {
                return false;
            }
        }

        private void SaveCCData(CCDataPoint d) {
            try {
                //IDictionary<String, Object> dictionary = d.GetType()
                //  .GetProperties()
                //  .Where(p => p.CanRead)
                //  .ToDictionary(p => p.Name, p => p.GetValue(d, null));
                //foreach (var dd in dictionary) Console.WriteLine($"{dd}");
                if (BuildTable()) {
                    _table.InsertRow(new BigQueryInsertRow
                    {
                        { "CycleNumber", d.CycleNumber },
                        { "LogTime_Timestamp", d.LogTime },
                        { "TotalTest_Hours", d.TotalTime },
                        { "MinutesIntoCycle", d.TimeIntoCycle },
                        { "CurrentBiasIsOn", d.CurrentBias ? "ON" : "OFF" },
                        { "SampleName", d.SampleName },
                        { "Current_Amps", d.Current },
                        { "Voltage_Volts", d.Voltage },
                        { "NumCells", d.NumCells },
                        { "CellVoc_Volts", d.CellVoc },
                        { "TempSensorNumber", d.TempSensor },
                        { "SetCurrent_Amps", d.SetCurrent },
                        { "EstimatedRs_mOhms", d.EstimatedRs },
                        { "Temp1_C", d.Temps[0] },
                        { "Temp2_C", d.Temps[1] },
                        { "Temp3_C", d.Temps[2] },
                        { "Temp4_C", d.Temps[3] },
                        { "Temp5_C", d.Temps[4] },
                        { "Temp6_C", d.Temps[5] },
                        { "Temp7_C", d.Temps[6] },
                        { "Temp8_C", d.Temps[7] },
                        { "Temp9_C", d.Temps[8] },
                        { "Temp10_C", d.Temps[9] },
                        { "Temp11_C", d.Temps[10] },
                        { "Temp12_C", d.Temps[11] },
                        { "Temp13_C", d.Temps[12] },
                        { "Temp14_C", d.Temps[13] },
                        { "Temp15_C", d.Temps[14] },
                        { "Temp16_C", d.Temps[15] },
                        { "SmokeLevel1_Volts", d.SmokeLevel[0] },
                        { "SmokeLevel2_Volts", d.SmokeLevel[1] },
                        { "SmokeLevel3_Volts", d.SmokeLevel[2] },
                        { "SmokeLevel4_Volts", d.SmokeLevel[3] },
                        { "SmokeLevel5_Volts", d.SmokeLevel[4] },
                        { "SmokeLevel6_Volts", d.SmokeLevel[5] },
                        { "SmokeLevel7_Volts", d.SmokeLevel[6] },
                        { "SmokeLevel8_Volts", d.SmokeLevel[7] },
                        { "SmokeVoltage1_Volts", d.SmokeVoltage[0] },
                        { "SmokeVoltage2_Volts", d.SmokeVoltage[1] },
                        { "SmokeVoltage3_Volts", d.SmokeVoltage[2] },
                        { "SmokeVoltage4_Volts", d.SmokeVoltage[3] },
                        { "SmokeVoltage5_Volts", d.SmokeVoltage[4] },
                        { "SmokeVoltage6_Volts", d.SmokeVoltage[5] },
                        { "SmokeVoltage7_Volts", d.SmokeVoltage[6] },
                        { "SmokeVoltage8_Volts", d.SmokeVoltage[7] },
                    });
                }
                else { throw new Exception(); }
            }
            catch { 
                Console.WriteLine("Google BQ connection ERROR. Backing up via CSV");
                SaveResults(d);
            }
        }

        private void SaveResults(CCDataPoint p) {
            var str = CompileDataStr(p);
            var path = $".\\backupData\\" + $"{p.SampleName}.txt";
            if (!File.Exists(path)) {
                using (var writer = new StreamWriter(path, true)) {
                    writer.WriteLine(U.SampleTxtHeader);
                }
            }
            using (var writer = new StreamWriter(path, true)) {
                writer.WriteLine(str);
            }
        }

        private string CompileDataStr(CCDataPoint p) {
            string[] str = { p.CycleNumber.ToString(), (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds.ToString(),
                p.TotalTime.ToString(), // total time (hrs)
                p.TimeIntoCycle.ToString(), // time into current cycle
                (p.CurrentBias) ? "ON" : "OFF",
                p.SampleName, p.Current.ToString(), p.Voltage.ToString(), p.NumCells.ToString(), p.CellVoc.ToString(),
                p.TempSensor.ToString(), p.SetCurrent.ToString(), "-99.99",
                $"{p.Temps[0].ToString("F2")}",$"{p.Temps[1].ToString("F2")}",$"{p.Temps[2].ToString("F2")}",
                $"{p.Temps[3].ToString("F2")}",$"{p.Temps[4].ToString("F2")}",$"{p.Temps[5].ToString("F2")}",
                $"{p.Temps[6].ToString("F2")}",$"{p.Temps[7].ToString("F2")}",$"{p.Temps[8].ToString("F2")}",
                $"{p.Temps[9].ToString("F2")}",$"{p.Temps[10].ToString("F2")}",$"{p.Temps[11].ToString("F2")}",
                $"{p.Temps[12].ToString("F2")}",$"{p.Temps[13].ToString("F2")}",$"{p.Temps[14].ToString("F2")}",
                $"{p.Temps[15].ToString("F2")}",
                $"{p.SmokeVoltage[0].ToString("F2")}", $"{p.SmokeVoltage[1].ToString("F2")}", $"{p.SmokeVoltage[2].ToString("F2")}",
                $"{p.SmokeVoltage[3].ToString("F2")}", $"{p.SmokeVoltage[4].ToString("F2")}", $"{p.SmokeVoltage[5].ToString("F2")}",
                $"{p.SmokeVoltage[6].ToString("F2")}", $"{p.SmokeVoltage[7].ToString("F2")}",
                $"{p.SmokeLevel[0].ToString("F2")}", $"{p.SmokeLevel[1].ToString("F2")}", $"{p.SmokeLevel[2].ToString("F2")}",
                $"{p.SmokeLevel[3].ToString("F2")}", $"{p.SmokeLevel[4].ToString("F2")}", $"{p.SmokeLevel[5].ToString("F2")}",
                $"{p.SmokeLevel[6].ToString("F2")}", $"{p.SmokeLevel[7].ToString("F2")}"};
            return string.Join(",", str);
        }

    }
}

