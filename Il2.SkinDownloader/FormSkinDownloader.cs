using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using IL2SkinDownloader.Core;
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
        private List<DiffManager.DiffInfo> _diffs;

        public FormSkinDownloader()
        {
            InitializeComponent();
            buttonCheckUpdates.Enabled = false;
            _configuration = GetCurrentConfiguration();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;

            listViewDiffs.Columns.AddRange(new[]
            {
                new ColumnHeader {Name = "FileName",Text = "Plane",Width = 0},
                new ColumnHeader {Name = "Status",Text = "Plane",Width = 0},
                new ColumnHeader {Name = "Last Update",Text = "Last Update",Width = 0},
                new ColumnHeader {Name = "Current",Text = "Current",Width = 0},
            });

            var imageList = new ImageList { ImageSize = new Size(20, 20) };
            imageList.Images.Add(Status.Added.ToString(), new Bitmap(Properties.Resources.plane_red));
            imageList.Images.Add(Status.Updated.ToString(), new Bitmap(Properties.Resources.plane_yellow));
            listViewDiffs.SmallImageList = imageList;


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


        private void LogChanged(int? percentage, string message)
        {
            label_Status.Text = message;
            if (percentage.HasValue)
            {
                labelPercentage.Text = $"{percentage.Value}%";
                progressBarSkinDownload.Increment(percentage.Value - progressBarSkinDownload.Value);
            }
        }
        private async void ButtonCheckUpdates_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(_configuration.Il2Path))
            {
                MessageBox.Show($"IL2 Installation path {_configuration.Il2Path} not found!");
                return;
            }

            if (_diffManager == null)
                _diffManager = new DiffManager(new GoogleDriveSkinDrive(), _configuration.Il2Path);

            label_Status.Text = "Check updates";
            _diffs = await _diffManager.GetDiffAsync(onProgress: LogChanged);
            PopulateListView(_diffs);
            label_Status.Text = "OK";
        }

        private void PopulateListView(IReadOnlyCollection<DiffManager.DiffInfo> diffs)
        {
            listViewDiffs.Items.Clear();

            var groups = diffs.Select(x => x.GroupId)
                .Distinct()
                .Select(x => new ListViewGroup(x, HorizontalAlignment.Left))
                .ToDictionary(x => x.Header);

            var items = diffs.Select((i, index) =>
            {
                var status = i.GetStatus();
                var item = new ListViewItem(i.Remote.Name) { ImageKey = status.ToString(), Tag = index };
                if (groups.TryGetValue(i.GroupId, out var group))
                {
                    item.Group = @group;
                }

                item.SubItems.Add(status.ToString());
                item.SubItems.Add(i.Remote?.LastUpdate.ToString("g") ?? "-");
                item.SubItems.Add(i.Local?.LastUpdate.ToString("g") ?? "-");
                return item;
            }).ToArray();

            listViewDiffs.Groups.AddRange(groups.Values.ToArray());
            listViewDiffs.Items.AddRange(items);
            listViewDiffs.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            var worker = sender as BackgroundWorker;
            worker.ReportProgress(0, $"Connecting to skin drive...");




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

        private void listViewDiffs_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (!buttonExec.Enabled && listViewDiffs.CheckedItems.Count > 0)
                buttonExec.Enabled = true;
            else if (buttonExec.Enabled && listViewDiffs.CheckedItems.Count == 0)
                buttonExec.Enabled = false;
        }

        public void UpdateProgress(int progress)
        {
            if (progressBarSkinDownload.InvokeRequired)
                progressBarSkinDownload.BeginInvoke(new Action(() => progressBarSkinDownload.Value = progress));
            else
                progressBarSkinDownload.Value = progress;

        }

        private async void buttonExec_Click(object sender, EventArgs e)
        {
            if (!_diffs.Any() || listViewDiffs.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please selected the items that you would be download.");
                return;
            }

            progressBarSkinDownload.Value = 0;

            var diffs = listViewDiffs.CheckedItems
                .OfType<ListViewItem>()
                .Select(x => _diffs[(int)x.Tag])
                .ToList();

            await _diffManager.ExecuteDiff(diffs, (progressStatus, diff) => UpdateProgress(progressStatus.Percentage));
        }
    }
}
