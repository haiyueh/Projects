using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace Current_Cycling_Controls {
    public class CCDataPoint {
        //public ObjectId Id { get; set; }
        public DateTime LogTime { get; set; }
        public int CycleNumber { get; set; }
        public double Epoch { get; set; }
        public double TotalTime { get; set; }
        public double TimeIntoCycle { get; set; }
        public bool CurrentBias { get; set; }
        public double Current { get; set; }
        public double Voltage { get; set; }
        public double EstimatedRs { get; set; }
        public List<double> Temps { get; set; }
        public List<double> SmokeVoltage { get; set; }
        public List<double> SmokeLevel { get; set; }

        // recipe parameters
        public string SampleName { get; set; }
        public int NumCells { get; set; }
        public double CellVoc { get; set; }
        public int TempSensor { get; set; }
        public double SetCurrent { get; set; }
        public int LastActivePort { get; set; }

        public CCDataPoint() { }
        public CCDataPoint(int cycle, double ep, double totaltime, double timeinto, bool bias, string sample,
            double curr, double voltage, int numcells, double cellVoc, int tempsensor, double setcurrent,
            double rs, List<double> temps, List<double> smokevolts, List<double> smokelevel) {
            //Id = ObjectId.GenerateNewId();
            LogTime = DateTime.UtcNow;
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
