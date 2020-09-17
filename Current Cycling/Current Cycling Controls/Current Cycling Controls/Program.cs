using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Current_Cycling_Controls
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if DEBUG
            
#else
            string process = null;
            switch (Environment.MachineName.ToUpper()) {
                case "SV-1F8HW33":
                    process = @"C:\Users\phoge\source\repos\Projects\Current Cycling\Current Cycling Controls\cc-copy.bat";
                    break;
            }

            if (process != null) {
                try {
                    Process.Start(process);
                }
                catch { }
            }
#endif


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
