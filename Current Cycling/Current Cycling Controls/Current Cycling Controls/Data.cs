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

        private void SaveProfilData(CCDataPoint d) {
            try {
                var col = _ccTable.GetCollection<CCDataPoint>(U.CCTable);
                col.InsertOne(d);
            }
            catch { }
        }
    }
}

