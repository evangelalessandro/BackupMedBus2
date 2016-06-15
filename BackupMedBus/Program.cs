using System;
using System.Windows.Forms;

namespace BackupMedBus
{
    internal static class Program
    {
        /// <summary>
        /// Punto di ingres  so principale dell'applicazione.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}