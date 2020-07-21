using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Current_Cycling_Controls {

    public class CCRecipe : Recipe<CCRecipe> {
        public string SampleName { get; set; }
        public int NumCells { get; set; }
        public double CellVoc { get; set; }
        public int TempSensor { get; set; }
        public double SetCurrent { get; set; }
        public int CycleNumber { get; set; }

        public CCRecipe() {
            Table = U.CCRecipeTable;
            Name = $"CC Parameters [{DateTime.Now:G}]";
            SampleName = "Sample not chosen";

        }

    }
}
