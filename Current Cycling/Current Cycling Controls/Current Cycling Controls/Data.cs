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
        private static readonly BigQueryClient client = BigQueryClient.Create("booming-pride-278623");
        private readonly BigQueryDataset dataset;

        private readonly Queue<DataQueueObject> _dataQueue = new Queue<DataQueueObject>();
        private readonly BackgroundWorker _dataWorker = new BackgroundWorker();
        private readonly AutoResetEvent _reset = new AutoResetEvent(false);
        private readonly object _lock = new object();

        public Data() {
            // Create the dataset if it doesn't exist.
            dataset = client.GetOrCreateDataset("TestBQ");
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

        public List<TData> GetRecipeList<TData>() {
            var list = new List<TData>();
            try {
                //var col = _ccTable.GetCollection<TData>(table);
                //var sort = Builders<TData>.Sort.Descending("_id");
                //var filter = new BsonDocument("Active", true);
                //list = col.Find(filter).Sort(sort).ToList();

                BigQueryTable table = dataset.GetTable("Test_Recipe");
                string sql = $"SELECT * FROM {table} ORDER BY LogTime DESC";
                var result = client.ExecuteQuery(sql, parameters: null).ToList();

            }

            catch (TimeoutException) {
            }
            catch (Exception exc) {
                Console.WriteLine(exc);
            }

            return list;
        }

        public TData GetCurrentRecipe<TData>(string table) {
            try {
                //var col = _ccTable.GetCollection<TData>(table);
                //var sort = Builders<TData>.Sort.Descending("_id");
                //var filter = Builders<TData>.Filter.And(new BsonDocument("Selected", true),
                //    new BsonDocument("Active", true));
                ////filter.And(new BsonDocument("Selected", true), new BsonDocument("Active", true));
                //return col.Find(filter).Sort(sort).FirstOrDefault();


            }
            catch (TimeoutException) {
            }
            catch (Exception exc) {
                Console.WriteLine(exc);
            }

            return default(TData);
        }

        public bool GetCurrentRecipeWithError<TData>(string table, out TData data) {
            data = default(TData);
            try {
                //var col = _ccTable.GetCollection<TData>(table);
                //var sort = Builders<TData>.Sort.Descending("_id");
                //var filter = Builders<TData>.Filter.And(new BsonDocument("Selected", true),
                //    new BsonDocument("Active", true));
                //data = col.Find(filter).Sort(sort).First();



                return true;
            }
            catch (TimeoutException) {
            }
            catch (Exception exc) {
                //Console.WriteLine(exc);
            }

            return false;
        }

        public void SaveData<TData>(Recipe<TData> data) {
            try {
                //var col = _ccTable.GetCollection<Recipe<TData>>(data.Table);
                //var filter = new BsonDocument("_id", data.Id);
                //var update = col.Find(filter).ToList().Count > 0;
                //data.Updated = DateTime.UtcNow;
                //if (update) {
                //    col.FindOneAndReplace(filter, data);
                //    return;
                //}
                //col.InsertOne(data);




            }
            catch (TimeoutException) { }
        }

        public void DeleteData<TData>(Recipe<TData> data) {
            try {
                //data.Active = false;
                //data.Updated = DateTime.UtcNow;
                //var col = _ccTable.GetCollection<Recipe<TData>>(data.Table);
                //var filter = new BsonDocument("_id", data.Id);
                //col.FindOneAndReplace(filter, data);
            }
            catch (TimeoutException) { }
        }

        /// <summary>
        /// Sets the selected 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="data"></param>
        public void SelectData<TData>(Recipe<TData> data) {
            try {
                //data.Selected = true;
                //data.Updated = DateTime.UtcNow;
                //var col = _ccTable.GetCollection<Recipe<TData>>(data.Table);
                //var filter = new BsonDocument();
                //var update = new UpdateDefinitionBuilder<Recipe<TData>>();
                //col.UpdateMany(filter, update.Set("Selected", false));
                //filter = new BsonDocument("_id", data.Id);
                //col.FindOneAndReplaceAsync(filter, data);
            }
            catch (TimeoutException) { }
        }

        private void SaveCCData(CCDataPoint d) {
            try {
                //var col = _ccTable.GetCollection<CCDataPoint>(U.CCTable);
                //col.InsertOne(d);
                BigQueryTable table = dataset.GetOrCreateTable("Test_Table_4", new TableSchemaBuilder
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
                            //{ "LastActivePort", BigQueryDbType.Array },

                        }.Build());

                table.InsertRow(new BigQueryInsertRow
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
                            { "Temps", d.Temps },
                            { "SmokeVoltage", d.SmokeVoltage },
                            { "SmokeLevel", d.SmokeLevel },
                            { "NumCells", d.NumCells },
                            { "CellVoc", d.CellVoc },
                            { "TempSensor", d.TempSensor },
                            { "SetCurrent", d.SetCurrent },
                        });
            }
            catch (Exception exc) { Console.WriteLine($"{exc}"); }
        }

        //private void GetLastEntry(DataQueueObject d) {
        //    try {
        //        var col = _ccTable.GetCollection<CCDataPoint>(U.CCTable);
        //        string sample = (string)d.Data.GetType().GetProperty("SampleName").GetValue(d.Data);
        //        // if WaferCounter equal to any in list of N/C/S counts
        //        var filter = Builders<CCDataPoint>.Filter.Eq("BondCounter", sample);
        //        // then set BondCounter to the last bondCount
        //        var update = Builders<WaferDataPoint>.Update.Set("BondCounter", bondCnt);
        //        col.UpdateMany(filter, update);
        //        col.S
        //    }
        //    catch { }
        //}
    }
}

