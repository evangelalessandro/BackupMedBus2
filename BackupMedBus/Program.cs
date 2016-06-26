using System;
using System.Linq;
using System.Windows.Forms;

namespace BackupMedBus
{
    internal static class Program
    {
        /// <summary>
        /// Punto di ingres  so principale dell'applicazione.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            if (Environment.GetCommandLineArgs().Length > 0
                & Environment.GetCommandLineArgs().First() == "backup")
            {
                ///backup from comment line
                using (var newbackup = new MakeBackup())
                {
                    newbackup.MakeNewBackup(null);
                }
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
        }
    }
}