using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BackupMedBus
{
    public partial class MainForm : Form
    {
        #region Public Constructors

        /// <summary>
        /// Main form
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        #endregion Public Constructors

        #region Public Methods

        public void AppendText(string text, Color color, Font font)
        {
            var box = RtbboxLog;
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.SelectionFont = font;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }

        public void AppendTextBox(LogEventArgs value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<LogEventArgs>(AppendTextBox), new object[] { value });
                return;
            }
            Application.DoEvents();
            var color = Color.Black;
            switch (value.State)
            {
                case LogWarning.Info:
                    color = Color.Black;
                    break;

                case LogWarning.Ok:
                    color = Color.Blue;
                    break;

                case LogWarning.Warning:
                    color = Color.Orange;
                    break;

                case LogWarning.Error:
                    color = Color.Red;
                    break;

                default:
                    break;
            }
            var fontStyleLog = new Font("Microsoft Sans Serif", 9, FontStyle.Bold);
            AppendText(DateTime.Now.ToString("hh:mm:ss fff"), color, fontStyleLog);
            AppendText(" " + value.Log + Environment.NewLine, color, fontStyleLog);
        }

        #endregion Public Methods

        #region Private Methods

        private void button1_Click(object sender, EventArgs e)
        {
            var exetoTest = ConfigurationManager.AppSettings["exeToTest"];
            if (IsProcessOpen(exetoTest))
            {
                MessageBox.Show(string.Format("Close the program {0} first to start backup", ConfigurationManager.AppSettings["ProgramName"]), "Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            List<DriveInfo> driveToBackup = new List<DriveInfo>();
            var usbDrive = System.IO.DriveInfo.GetDrives()
                .Where(a => a.DriveType == System.IO.DriveType.Removable).ToList();
            if (usbDrive.Count > 0)
            {
                foreach (var item in usbDrive)
                {
                    if (MessageBox.Show("Do you want to backup also in " + item.ToString(), "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                        == DialogResult.Yes)
                    {
                        driveToBackup.Add(item);
                    }
                }
            }
            Cursor.Current = Cursors.WaitCursor;

            button1.Enabled = false;
            RtbboxLog.Clear();
            imgProgress.Image = Properties.Resources.yellow;
            try
            {
                using (var newbackup = new MakeBackup())
                {
                    newbackup.Log += Newbackup_Log;
                    newbackup.MakeNewBackup(driveToBackup);
                }
                imgProgress.Image = Properties.Resources.green;
            }
            catch (Exception ex)
            {
                AppendTextBox(
                    new LogEventArgs(
                        ex.Message + " " + ex.StackTrace.ToString(), LogWarning.Error));

                AppendTextBox(
                new LogEventArgs(
                    "----- Error --------------------", LogWarning.Error));
                imgProgress.Image = Properties.Resources.Red;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                button1.Enabled = true;
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            RtbboxLog.Clear();

            button2.Enabled = false;
            imgProgress.Image = Properties.Resources.yellow;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                string dir = ConfigurationManager.AppSettings["SourceDirectory"].ToString();

                using (var loadDb = new Report.LoadFromDatabases())
                {
                    loadDb.Log += Newbackup_Log;
                    loadDb.ReadTables(dir);
                }
                imgProgress.Image = Properties.Resources.green;
            }
            catch (Exception ex)
            {
                AppendTextBox(
                    new LogEventArgs(
                        ex.Message + " " + ex.StackTrace.ToString(), LogWarning.Error));

                AppendTextBox(
                new LogEventArgs(
                    "----- Error --------------------", LogWarning.Error));
                imgProgress.Image = Properties.Resources.yellow;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                button2.Enabled = true;
            }
        }

        private bool IsProcessOpen(string name)
        {
            var list = Process.GetProcessesByName(name);
            if (list.Count() > 0)
            {
                return true;
            }
            return false;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        private void Newbackup_Log(object sender, LogEventArgs e)
        {
            AppendTextBox(e);
        }

        #endregion Private Methods
    }
}