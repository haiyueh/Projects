using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Current_Cycling_Controls {
    [BsonIgnoreExtraElements]

    public class CCRecipe : Recipe<CCRecipe> {
        public string SampleName { get; set; }
        public int NumCells { get; set; }
        public double CellVoc { get; set; }
        public int TempSensor { get; set; }
        public double SetCurrent { get; set; }
        public int LastActivePort { get; set; }

        public CCRecipe() {
            Table = U.CCRecipeTable;
            Name = $"CC Parameters [{DateTime.Now:G}]";
        }

    }
}
