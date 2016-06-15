using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace BackupMedBus
{
    /// <summary>
    /// classe per il backup
    /// </summary>
    public class MakeBackup : IDisposable
    {
        public event EventHandler<LogEventArgs> Log;

        public void MakeNewBackup(List<DriveInfo> driveToBackup)
        {
            string sourceFolder = CheckDirectory(
                ConfigurationManager.AppSettings["SourceDirectory"]);

            string destination01 = CheckDirectory(
                ConfigurationManager.AppSettings["Destination01"]);

            //NameFolderDestinationFormatDate

            var formatDataFolderName = ConfigurationManager.AppSettings["NameFolderDestinationFormatDate"];
            var prefixFolder = "";
            if (!string.IsNullOrEmpty(formatDataFolderName))
            {
                prefixFolder = DateTime.Now.ToString(formatDataFolderName)
                     + @"\";

                Log(this,
                new LogEventArgs
                ("Prefix folder Name " + prefixFolder, LogEventArgs.LogWarning.Info));
            }

            string destination02 = CheckDirectory(
                ConfigurationManager.AppSettings["Destination02"]);

            string destination03 = CheckDirectory(
                ConfigurationManager.AppSettings["Destination03"]);

            if (prefixFolder.Length > 0)
            {
                Copia(driveToBackup, sourceFolder, destination01, @"LastCopy\", destination02, destination03);
                Copia(driveToBackup, sourceFolder, destination01, prefixFolder, destination02, destination03);
            }
            else
            {
                Copia(driveToBackup, sourceFolder, destination01, "", destination02, destination03);
            }
        }

        private void Copia(List<DriveInfo> driveToBackup, string sourceFolder, string destination01, string prefixFolder, string destination02, string destination03)
        {
            Esegui(sourceFolder, destination01 + prefixFolder);
            if (destination02.Length > 0)
            {
                Esegui(sourceFolder, destination02 + prefixFolder);
            }
            if (destination03.Length > 0)
            {
                Esegui(sourceFolder, destination03 + prefixFolder);
            }
            foreach (var item in driveToBackup)
            {
                Esegui(sourceFolder, Path.Combine(item.Name, @"Backup Patient\", prefixFolder));
            }
        }

        private string CheckDirectory(string sourceFolder)
        {
            if (string.IsNullOrEmpty(sourceFolder))
            {
                return sourceFolder;
            }
            if (sourceFolder.Last().ToString() != @"\")
            {
                return sourceFolder + @"\";
            }
            return sourceFolder;
        }

        private void Esegui(string sourceFolder, string destination)
        {
            if (string.IsNullOrEmpty(destination))
            {
                return;
            }
            CopyAll(sourceFolder, destination);
        }

        //For this, we can use any 3rd party tool.Hobocopy is one of them.It will start the two services automatically when needed, and the Volume Shadow Copy service will be turned back off after it's done.
        //Download Link: https://github.com/candera/hobocopy/downloads

        //Unzip the file and place them into Lib directory of your project. Then call the process using System.Diagnostics.Process. In your case, just use the following switch as argument: /y(Don't prompt, just copy everything)

        //Command-Line Syntax:

        //hobocopy[/ statefile = FILE][/ verbosity = LEVEL][ / full | / incremental]
        //    [ / clear][ / skipdenied][ / y][ / simulate][/ recursive]
        //    src dest[file[file[ ... ]]
        //C# code:

        //Process process = new Process();
        //process.StartInfo.UseShellExecute = false;
        //process.StartInfo.CreateNoWindow = true;
        //process.StartInfo.FileName = @"Lib\hobocopy.exe";
        //process.StartInfo.Arguments = @" /y <network-directory> <local-directory> <filename>";
        //process.Start();
        //This'll copy any files (hobocopy focuses mainly on copying directories efficiently) that is being used or unused to your destination.

        private void hobocopy()
        {
            //Process process = new Process();
            //process.StartInfo.UseShellExecute = false;
            //process.StartInfo.CreateNoWindow = true;

            //process.StartInfo.FileName = System.IO.Path.Combine( Environment.CurrentDirectory , @"Lib\hobocopy.exe");

            //process.StartInfo.Arguments = @" " + sourceFolder
            //    + " " +  destination + " /E /S";
            //process.Start();
        }

        private void CopyAll(string SourcePath, string DestinationPath)
        {
            string[] directories = System.IO.Directory.GetDirectories(SourcePath, "*.*", SearchOption.AllDirectories);
            Log(this,
                new LogEventArgs
                ("Copy " + SourcePath + " to " + DestinationPath, LogEventArgs.LogWarning.Ok));

            Log(this,
                new LogEventArgs
                ("-------------------------------", LogEventArgs.LogWarning.Info));

            foreach (var dirPath in directories)
            {
                string dirName = dirPath.Replace(SourcePath, DestinationPath);

                Log(this,
                new LogEventArgs
                ("Create dir " + dirPath.Replace(SourcePath, ""), LogEventArgs.LogWarning.Ok));

                Directory.CreateDirectory(dirName);
            };

            string[] files = System.IO.Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories);

            foreach (var newPath in files)
            {
                var fileDest = newPath.Replace(SourcePath, DestinationPath);

                Log(this,
                    new LogEventArgs
                            ("Copy File ..." + fileDest.Replace(DestinationPath, ""), LogEventArgs.LogWarning.Ok));

                File.Copy(newPath, fileDest, true);
            }

            Log(this,
                new LogEventArgs
                (" OK -------------------------------", LogEventArgs.LogWarning.Info));
        }

        public void Dispose()
        {
        }
    }
}