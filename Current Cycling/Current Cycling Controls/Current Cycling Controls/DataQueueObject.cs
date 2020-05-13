using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Current_Cycling_Controls {
    public class DataQueueObject {
        public DataType Type { get; set; }
        public object Data { get; set; }
        public bool Update { get; set; }

        public DataQueueObject(DataType t, object d, bool update = false) {
            Type = t;
            Data = d;
            Update = update;
        }

        public enum DataType {
            NoData,
            CycleData

        }
    }
}
