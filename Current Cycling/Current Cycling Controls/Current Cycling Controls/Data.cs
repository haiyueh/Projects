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

namespace Current_Cycling_Controls {

    public delegate void DataEvent(object sender, DataQueueObject e);
    public class Data {

#if DEBUG
        private static readonly MongoClient Mongo = new MongoClient(@"mongodb://127.0.0.1:27017/?compressors=disabled&gssapiServiceName=mongodb");
#else
        private static readonly MongoClient Mongo = new MongoClient(@"mongodb://127.0.0.1:27017/?compressors=disabled&gssapiServiceName=mongodb");
        //private static readonly MongoClient Mongo = new MongoClient(@"mongodb://10.10.10.5:27017");
#endif

        private readonly Queue<DataQueueObject> _dataQueue = new Queue<DataQueueObject>();
        private readonly BackgroundWorker _dataWorker = new BackgroundWorker();
        private readonly AutoResetEvent _reset = new AutoResetEvent(false);
        private readonly IMongoDatabase _ccTable;
        private readonly object _lock = new object();

        public Data() {
            _ccTable = Mongo.GetDatabase(U.CCdb);
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
                    SaveProfilData(p);
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

        public List<TData> GetRecipeList<TData>(string table) {
            var list = new List<TData>();
            try {
                var col = _ccTable.GetCollection<TData>(table);
                var sort = Builders<TData>.Sort.Descending("_id");
                var filter = new BsonDocument("Active", true);
                list = col.Find(filter).Sort(sort).ToList();

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
                var col = _ccTable.GetCollection<TData>(table);
                var sort = Builders<TData>.Sort.Descending("_id");
                var filter = Builders<TData>.Filter.And(new BsonDocument("Selected", true),
                    new BsonDocument("Active", true));
                //filter.And(new BsonDocument("Selected", true), new BsonDocument("Active", true));
                return col.Find(filter).Sort(sort).FirstOrDefault();
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
                var col = _ccTable.GetCollection<TData>(table);
                var sort = Builders<TData>.Sort.Descending("_id");
                var filter = Builders<TData>.Filter.And(new BsonDocument("Selected", true),
                    new BsonDocument("Active", true));
                data = col.Find(filter).Sort(sort).First();
                return true;
            }
            catch (TimeoutException) {
            }
            catch (Exception exc) {
                //Console.WriteLine(exc);
            }

            return false;
        }

        public TData GetRecipeById<TData>(string table, ObjectId id) {
            try {
                var col = _ccTable.GetCollection<TData>(table);
                var filter = new BsonDocument("_id", id);
                return col.Find(filter.ToBsonDocument()).First();
            }
            catch (TimeoutException) {
            }
            catch (Exception exc) {
                Console.WriteLine(exc);
            }

            return default(TData);
        }

        public void SaveData<TData>(Recipe<TData> data) {
            try {
                var col = _ccTable.GetCollection<Recipe<TData>>(data.Table);
                var filter = new BsonDocument("_id", data.Id);
                var update = col.Find(filter).ToList().Count > 0;
                data.Updated = DateTime.UtcNow;
                if (update) {
                    col.FindOneAndReplace(filter, data);
                    return;
                }
                col.InsertOne(data);
            }
            catch (TimeoutException) { }
        }

        public void DeleteData<TData>(Recipe<TData> data) {
            try {
                data.Active = false;
                data.Updated = DateTime.UtcNow;
                var col = _ccTable.GetCollection<Recipe<TData>>(data.Table);
                var filter = new BsonDocument("_id", data.Id);
                col.FindOneAndReplace(filter, data);
            }
            catch (TimeoutException) { }
        }

        public void SelectData<TData>(Recipe<TData> data) {
            try {
                data.Selected = true;
                data.Updated = DateTime.UtcNow;
                var col = _ccTable.GetCollection<Recipe<TData>>(data.Table);
                var filter = new BsonDocument();
                var update = new UpdateDefinitionBuilder<Recipe<TData>>();
                col.UpdateMany(filter, update.Set("Selected", false));
                filter = new BsonDocument("_id", data.Id);
                col.FindOneAndReplaceAsync(filter, data);
            }
            catch (TimeoutException) { }
        }

        private void SaveProfilData(CCDataPoint d) {
            try {
                var col = _ccTable.GetCollection<CCDataPoint>(U.CCTable);
                col.InsertOne(d);
            }
            catch { }
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

