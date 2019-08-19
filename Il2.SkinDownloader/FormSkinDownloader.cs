using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Il2.GreatBattles;
using Il2.RemoteDrive;
using Newtonsoft.Json;

namespace Il2SkinDownloader
{
    public partial class FormSkinDownloader : Form
    {
        private Il2Game _il2;
        private const string SettingsFileName = "settings.json";
        private Configuration _configuration;
        private GoogleDrive _googleDrive;

        public FormSkinDownloader()
        {
            InitializeComponent();
            buttonCheckUpdates.Enabled = false;
            _configuration = GetCurrentConfiguration();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            if (!string.IsNullOrWhiteSpace(_configuration.Il2Path))
            {
                textBox_Il2Path.Text = _configuration.Il2Path;
                buttonCheckUpdates.Enabled = true;
            }
        }

        private Configuration GetCurrentConfiguration()
        {
            if (File.Exists(SettingsFileName))
            {
                var fileContent = File.ReadAllText(SettingsFileName);
                var configFromFile = JsonConvert.DeserializeObject<Configuration>(fileContent);
                if (configFromFile != null)
                    return configFromFile;
            }

            return new Configuration();
        }

        private void SaveConfiguration(Configuration configuration)
        {
            if (configuration == null) return;
            var configurationString = JsonConvert.SerializeObject(configuration);
            File.WriteAllText(SettingsFileName, configurationString);
        }
        private void Button_OpenIl2Folder_Click(object sender, EventArgs e)
        {
            var openFolderDialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = false,
                Description = $"Select the Il2 folder path",
            };

            var result = openFolderDialog.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(openFolderDialog.SelectedPath))
            {
                var directoryInfo = new DirectoryInfo(openFolderDialog.SelectedPath);
                if (!Il2Game.Il2FolderNames.Contains(directoryInfo.Name))
                {
                    MessageBox.Show($"Selected path not valid. The folder must be a valid il2 installation path");
                    textBox_Il2Path.Text = null;
                    _configuration.Il2Path = null;
                    buttonCheckUpdates.Enabled = false;
                }
                else
                {
                    textBox_Il2Path.Text = directoryInfo.FullName;
                    _configuration.Il2Path = directoryInfo.FullName;
                    _il2 = new Il2Game(_configuration.Il2Path);
                    SaveConfiguration(_configuration);
                    buttonCheckUpdates.Enabled = true;
                }
            }
        }

        private void ButtonCheckUpdates_Click(object sender, EventArgs e)
        {
            if (backgroundWorker.IsBusy != true)
            {
                backgroundWorker.RunWorkerAsync();
            }
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            _il2 = new Il2Game(_configuration.Il2Path);
            worker.ReportProgress(0, $"Connecting to skin drive...");
            _googleDrive = new GoogleDrive("skinDownloader");
            _googleDrive.Connect(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "auth.json"));

            worker.ReportProgress(0, $"Download the skin informations...");
            var remoteItems = _googleDrive.GetFiles();
            var remoteFiles = remoteItems.Where(x => !x.IsFolder).ToArray();

            var localPlanesFolder = _il2.GetCustomSkinDirectories().Select(x => x.Name);
            var remoteFolders = remoteItems.Where(x => x.IsFolder && localPlanesFolder.Contains(x.Name)).ToArray();
            worker.ReportProgress(0, $"Checking skins to update...");
            var downloads = new List<(GoogleDriveItem remoteFile, string localPath)>();
            var toDelete = new List<FileInfo>();
            foreach (var remoteFolder in remoteFolders)
            {
                var remoteFilesForDirectory = remoteFiles
                    .Where(x => x.Parents == remoteFolder.Id)
                    .ToArray();

                var localFolderPath = Path.Combine(_il2.SkinDirectoryPath, remoteFolder.Name);

                var currentLocalFiles = Directory.EnumerateFiles(localFolderPath)
                    .Select(fileName => new FileInfo(fileName))
                    .ToArray();

                var del = currentLocalFiles
                   .Where(localFile => !remoteFilesForDirectory.Select(x => x.Name).Contains(localFile.Name))
                   .ToArray();

                toDelete.AddRange(del);

                foreach (var remoteFile in remoteFilesForDirectory)
                {
                    var existingFile = currentLocalFiles.FirstOrDefault(l => string.Equals(remoteFile.Name, l.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (existingFile == null || remoteFile.ModifiedTime > existingFile.LastWriteTime)
                    {
                        var currentFilePath = Path.Combine(localFolderPath, remoteFile.Name);
                        downloads.Add((remoteFile, currentFilePath));
                    }
                }
            }

            for (var i = 1; i <= downloads.Count; i++)
            {
                var (remoteFile, localPath) = downloads[i - 1];
                var ff = (int)Math.Floor((double)i / downloads.Count * 100);
                worker.ReportProgress((int)ff, $"Downloading {remoteFile.Name}");

                if (File.Exists(localPath))
                    File.Delete(localPath);

                _googleDrive.Download(remoteFile.Id, localPath);
            }

            foreach (var fileInfo in toDelete)
            {
                File.Delete(fileInfo.FullName);
            }

            worker.ReportProgress(100, $"Job completed");
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var status = e.UserState.ToString();
            label_Status.Text = status;
            labelPercentage.Text = $"{e.ProgressPercentage.ToString()}%";
            progressBarSkinDownload.Increment(e.ProgressPercentage - progressBarSkinDownload.Value);
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                label_Status.Text = "Job Canceled!";
            }
            else if (e.Error != null)
            {
                MessageBox.Show(e.Error.ToString(), "ERROR");
            }
            else
            {
                MessageBox.Show("Job completed!");
                Close();
            }
        }
    }

    public class Configuration
    {
        public string Il2Path { get; set; }
    }
}
