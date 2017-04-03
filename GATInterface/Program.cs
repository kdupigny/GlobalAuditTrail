using System;
using System.Windows.Forms;
using GATInterface.Forms;
using GATInterface.Forms.PositionData;
using GATUtils.Connection.DB;

namespace GATInterface
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new GatInterfaceDevX());
            Application.Run(new MainWindow());

            DbHandle.Instance.Dispose();
        }
    }
}
