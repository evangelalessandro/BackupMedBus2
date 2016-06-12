﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackupMedBus
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// Main form
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
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
                        ex.Message + " " + ex.StackTrace.ToString(), LogEventArgs.LogWarning.Error));

                AppendTextBox(
                new LogEventArgs(
                    "----- Error --------------------", LogEventArgs.LogWarning.Error));
                imgProgress.Image = Properties.Resources.Red;
            }
            button1.Enabled = true;
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
                case LogEventArgs.LogWarning.Info:
                    color = Color.Black;
                    break;
                case LogEventArgs.LogWarning.Ok:
                    color = Color.Blue;
                    break;
                case LogEventArgs.LogWarning.Warning:
                    color = Color.Orange;
                    break;
                case LogEventArgs.LogWarning.Error:
                    color = Color.Red;
                    break;
                default:
                    break;
            }
            var fontStyleLog = new Font("Microsoft Sans Serif", 9, FontStyle.Bold);
            AppendText(DateTime.Now.ToString("hh:mm:ss fff"), color, fontStyleLog);
            AppendText(" " + value.Log + Environment.NewLine, color, fontStyleLog);

        }
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
        private void Newbackup_Log(object sender, LogEventArgs e)
        {
            AppendTextBox(e);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            imgProgress.Image = Properties.Resources.green;
        }
    }
}
