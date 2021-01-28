using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using IL2SkinDownloader.Core;
using IL2SkinDownloader.Core.GoogleDrive;
using IL2SkinDownloader.Core.IL2;
using Newtonsoft.Json;

namespace Il2SkinDownloader
{
    public partial class FormSkinDownloader : Form
    {
        private readonly string _folderSettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "IL2SkinDownloader");
        private string SettingsFileName => Path.Combine(_folderSettingsPath, "settings.json");
        private Configuration _configuration;
        private DiffManager _diffManager;

        public FormSkinDownloader()
        {
            InitializeComponent();
            buttonCheckUpdates.Enabled = false;
            _configuration = GetCurrentConfiguration();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            listViewDiffs.Columns.AddRange(new[]
            {
                new ColumnHeader {Name = "Plane",Text = "Plane"},
                new ColumnHeader {Name = "Status",Text = "Plane"},

            });
            listViewDiffs.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
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

            if (!Directory.Exists(_folderSettingsPath))
                Directory.CreateDirectory(_folderSettingsPath);

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
                if (!IL2Helpers.Il2FolderNames.Contains(directoryInfo.Name))
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
                    SaveConfiguration(_configuration);
                    buttonCheckUpdates.Enabled = true;
                }
            }
        }

        private async void ButtonCheckUpdates_Click(object sender, EventArgs e)
        {
            if (_diffManager == null)
                _diffManager = new DiffManager(new GoogleDriveSkinDrive(), _configuration.Il2Path);

            label_Status.Text = "Check updates";
            var diffs = await _diffManager.GetDiffAsync();
            var items = diffs.Select(d =>
            {
                var status = d.GetStatus();
                var item = new ListViewItem(d.Remote.Name);
                item.SubItems.Add(status.ToString());
                return item;
            }).ToArray();
            listViewDiffs.Items.AddRange(items);

            //await _diffManager.ExecuteDiff(diff);

            //if (backgroundWorker.IsBusy != true)
            //{
            //    backgroundWorker.RunWorkerAsync();
            //}
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            var worker = sender as BackgroundWorker;
            worker.ReportProgress(0, $"Connecting to skin drive...");


            var googleDriveWrapper = new GoogleDriveWrapper();
            googleDriveWrapper.Connect(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "auth.json"), "skinDownloader");

            worker.ReportProgress(0, $"Download the skin informations...");

            //var configuration = StaticConfiguration.GetCurrentConfiguration();
            //Installer.Install(configuration);

            var remoteItems = googleDriveWrapper.GetFilesAsync(CancellationToken.None).Result;
            var remoteFiles = remoteItems.Where(x => !x.IsFolder).ToArray();
            var localPlanesFolder = IL2Helpers.GetCustomSkinDirectories(_configuration.Il2Path).Select(x => x.Name);
            var remoteFolders = remoteItems.Where(x => x.IsFolder && localPlanesFolder.Contains(x.Name)).ToArray();
            worker.ReportProgress(0, $"Checking skins to update...");

            var downloads = new List<(GoogleDriveItem remoteFile, string localPath)>();
            var toDelete = new List<FileInfo>();
            foreach (var remoteFolder in remoteFolders)
            {
                var remoteFilesForDirectory = remoteFiles
                    .Where(x => x.Parents?.Contains(remoteFolder.Id, StringComparer.InvariantCultureIgnoreCase) ?? false)
                    .ToArray();

                var localFolderPath = Path.Combine(IL2Helpers.SkinDirectoryPath(_configuration.Il2Path), remoteFolder.Name);

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

                googleDriveWrapper.DownloadAsync(remoteFile.Id, localPath).Wait();
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

        private void FormSkinDownloader_Load(object sender, EventArgs e)
        {

        }
    }
}
