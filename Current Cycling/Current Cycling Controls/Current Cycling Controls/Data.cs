using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Google.Cloud.BigQuery.V2;
using Google.Apis.Bigquery.v2.Data;

namespace Current_Cycling_Controls {

    public delegate void DataEvent(object sender, DataQueueObject e);
    public class Data {
        private static readonly BigQueryClient _client = BigQueryClient.Create("booming-pride-278623");
        private readonly BigQueryDataset _dataset;
        private readonly BigQueryTable _table;

        private readonly Queue<DataQueueObject> _dataQueue = new Queue<DataQueueObject>();
        private readonly BackgroundWorker _dataWorker = new BackgroundWorker();
        private readonly AutoResetEvent _reset = new AutoResetEvent(false);
        private readonly object _lock = new object();

        public Data() {
            // Create the dataset if it doesn't exist.
            _dataset = _client.GetOrCreateDataset("CCDataset");
            _table = _dataset.GetOrCreateTable("CCTable", new TableSchemaBuilder
                {
                    { "LogTime", BigQueryDbType.Timestamp },
                    { "SampleName", BigQueryDbType.String },
                    { "CycleNumber", BigQueryDbType.Int64 },
                    { "Epoch", BigQueryDbType.Float64 },
                    { "TotalTime", BigQueryDbType.Float64 },
                    { "TimeIntoCycle", BigQueryDbType.Float64 },
                    { "CurrentBias", BigQueryDbType.Bool },
                    { "Current", BigQueryDbType.Float64 },
                    { "Voltage", BigQueryDbType.Float64 },
                    { "EstimatedRs", BigQueryDbType.Float64 },
                    { "Temp1", BigQueryDbType.Float64 },
                    { "Temp2", BigQueryDbType.Float64 },
                    { "Temp3", BigQueryDbType.Float64 },
                    { "Temp4", BigQueryDbType.Float64 },
                    { "Temp5", BigQueryDbType.Float64 },
                    { "Temp6", BigQueryDbType.Float64 },
                    { "Temp7", BigQueryDbType.Float64 },
                    { "Temp8", BigQueryDbType.Float64 },
                    { "Temp9", BigQueryDbType.Float64 },
                    { "Temp10", BigQueryDbType.Float64 },
                    { "Temp11", BigQueryDbType.Float64 },
                    { "Temp12", BigQueryDbType.Float64 },
                    { "SmokeLevel1", BigQueryDbType.Float64 },
                    { "SmokeLevel2", BigQueryDbType.Float64 },
                    { "SmokeLevel3", BigQueryDbType.Float64 },
                    { "SmokeLevel4", BigQueryDbType.Float64 },
                    { "SmokeLevel5", BigQueryDbType.Float64 },
                    { "SmokeLevel6", BigQueryDbType.Float64 },
                    { "SmokeLevel7", BigQueryDbType.Float64 },
                    { "SmokeLevel8", BigQueryDbType.Float64 },
                    { "SmokeVoltage1", BigQueryDbType.Float64 },
                    { "SmokeVoltage2", BigQueryDbType.Float64 },
                    { "SmokeVoltage3", BigQueryDbType.Float64 },
                    { "SmokeVoltage4", BigQueryDbType.Float64 },
                    { "SmokeVoltage5", BigQueryDbType.Float64 },
                    { "SmokeVoltage6", BigQueryDbType.Float64 },
                    { "SmokeVoltage7", BigQueryDbType.Float64 },
                    { "SmokeVoltage8", BigQueryDbType.Float64 },
                    { "NumCells", BigQueryDbType.Int64 },
                    { "CellVoc", BigQueryDbType.Float64 },
                    { "TempSensor", BigQueryDbType.Int64 },
                    { "SetCurrent", BigQueryDbType.Float64 },
                }.Build());
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

            return lst;
        }

        public CCRecipe GetCurrentRecipe(string currentSample) {
            var recipe = new CCRecipe();
            try {
                var sql = $"SELECT * FROM {_table} WHERE SampleName = \"{currentSample}\" ORDER BY LogTime DESC LIMIT 1";

                var results = _client.ExecuteQuery(sql, null);
                var row = results.FirstOrDefault();
                recipe = new CCRecipe() {
                    SampleName = row["SampleName"].ToString(),
                    NumCells = int.Parse(row["NumCells"].ToString()),
                    CellVoc = double.Parse(row["CellVoc"].ToString()),
                    TempSensor = int.Parse(row["TempSensor"].ToString()),
                    SetCurrent = double.Parse(row["SetCurrent"].ToString()),
                    CycleNumber = int.Parse(row["CycleNumber"].ToString()),
                };


            }
            catch (TimeoutException) {
            }
            catch (Exception exc) {
                Console.WriteLine(exc);
            }

            return recipe;
        }


        private void SaveCCData(CCDataPoint d) {
            try {
                //IDictionary<String, Object> dictionary = d.GetType()
                //  .GetProperties()
                //  .Where(p => p.CanRead)
                //  .ToDictionary(p => p.Name, p => p.GetValue(d, null));
                //foreach (var dd in dictionary) Console.WriteLine($"{dd}");
                _table.InsertRow(new BigQueryInsertRow
                {
                    { "LogTime", d.LogTime },
                    { "SampleName", d.SampleName },
                    { "CycleNumber", d.CycleNumber },
                    { "Epoch", d.Epoch },
                    { "TotalTime", d.TotalTime },
                    { "TimeIntoCycle", d.TimeIntoCycle },
                    { "CurrentBias", d.CurrentBias },
                    { "Current", d.Current },
                    { "Voltage", d.Voltage },
                    { "EstimatedRs", d.EstimatedRs },
                    { "Temp1", d.Temps[0] },
                    { "Temp2", d.Temps[1] },
                    { "Temp3", d.Temps[2] },
                    { "Temp4", d.Temps[3] },
                    { "Temp5", d.Temps[4] },
                    { "Temp6", d.Temps[5] },
                    { "Temp7", d.Temps[6] },
                    { "Temp8", d.Temps[7] },
                    { "Temp9", d.Temps[8] },
                    { "Temp10", d.Temps[9] },
                    { "Temp11", d.Temps[10] },
                    { "Temp12", d.Temps[11] },
                    { "SmokeLevel1", d.SmokeLevel[0] },
                    { "SmokeLevel2", d.SmokeLevel[1] },
                    { "SmokeLevel3", d.SmokeLevel[2] },
                    { "SmokeLevel4", d.SmokeLevel[3] },
                    { "SmokeLevel5", d.SmokeLevel[4] },
                    { "SmokeLevel6", d.SmokeLevel[5] },
                    { "SmokeLevel7", d.SmokeLevel[6] },
                    { "SmokeLevel8", d.SmokeLevel[7] },
                    { "SmokeVoltage1", d.SmokeVoltage[0] },
                    { "SmokeVoltage2", d.SmokeVoltage[1] },
                    { "SmokeVoltage3", d.SmokeVoltage[2] },
                    { "SmokeVoltage4", d.SmokeVoltage[3] },
                    { "SmokeVoltage5", d.SmokeVoltage[4] },
                    { "SmokeVoltage6", d.SmokeVoltage[5] },
                    { "SmokeVoltage7", d.SmokeVoltage[6] },
                    { "SmokeVoltage8", d.SmokeVoltage[7] },
                    { "NumCells", d.NumCells },
                    { "CellVoc", d.CellVoc },
                    { "TempSensor", d.TempSensor },
                    { "SetCurrent", d.SetCurrent },
                });
            }
            catch (Exception exc) { Console.WriteLine($"{exc}"); }
        }

        private void BtnQueryClick(object s, EventArgs e) {
            var table = _dataset.GetTable("Test_Data_Recipe");
            var sample = "Test1";
            var sql = $"SELECT * FROM {table} WHERE SampleName = \"{sample}\" ORDER BY LogTime DESC LIMIT 1";

            var results = _client.ExecuteQuery(sql, null);
            var row = results.FirstOrDefault();
            var lst = row["Temps"] as double[];

            CCDataPoint pt = new CCDataPoint() {
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

    }
}

