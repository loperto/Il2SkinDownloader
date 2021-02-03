using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        private List<DiffInfo> _diffs;

        public FormSkinDownloader()
        {
            InitializeComponent();
            buttonCheckUpdates.Enabled = false;
            _configuration = GetCurrentConfiguration();

            listViewDiffs.Columns.AddRange(new[]
            {
                new ColumnHeader {Name = "FileName",Text = "Plane",Width = 0},
                new ColumnHeader {Name = "Status",Text = "Status",Width = 0},
                new ColumnHeader {Name = "Size",Text = "Size",Width = 0},
                new ColumnHeader {Name = "Last Update",Text = "Last Update",Width = 0},
                new ColumnHeader {Name = "Current",Text = "Current",Width = 0},
            });

            var imageList = new ImageList { ImageSize = new Size(20, 20) };
            imageList.Images.Add(Status.Added.ToString(), new Bitmap(Properties.Resources.plane_green));
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

        private async void ButtonCheckUpdates_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(_configuration.Il2Path))
            {
                MessageBox.Show($"IL2 Installation path {_configuration.Il2Path} not found!");
                return;
            }

            await GetDiff();
        }

        private async Task GetDiff()
        {
            if (_diffManager == null)
                _diffManager = new DiffManager(new GoogleDriveSkinDrive(), _configuration.Il2Path);

            label_Status.Text = "Checking for updates...";
            _diffs = await _diffManager.GetDiffAsync();
            PopulateListView(_diffs);
            label_Status.Text = $"{_diffs.Count} updated items found";
        }

        private void PopulateListView(IReadOnlyCollection<DiffInfo> diffs)
        {
            listViewDiffs.Items.Clear();

            var groups = diffs.Select(x => x.GroupId)
                .Distinct()
                .Select(x => new ListViewGroup(x, HorizontalAlignment.Left))
                .ToDictionary(x => x.Header);

            var items = diffs.Select((i, index) =>
            {
                var item = new ListViewItem(i.Remote.Name) { ImageKey = i.Status.ToString(), Tag = index };
                if (groups.TryGetValue(i.GroupId, out var group))
                {
                    item.Group = @group;
                }

                item.SubItems.Add(i.Status.ToString());
                item.SubItems.Add(GetReadableSize(i.Remote.Size));
                item.SubItems.Add(i.Remote?.LastUpdate.ToString("g") ?? "-");
                item.SubItems.Add(i.Local?.LastUpdate.ToString("g") ?? "-");
                return item;
            }).ToArray();

            listViewDiffs.Groups.AddRange(groups.Values.ToArray());
            listViewDiffs.Items.AddRange(items);
            listViewDiffs.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void listViewDiffs_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (!buttonExec.Enabled && listViewDiffs.CheckedItems.Count > 0)
                buttonExec.Enabled = true;
            else if (buttonExec.Enabled && listViewDiffs.CheckedItems.Count == 0)
                buttonExec.Enabled = false;
        }

        public void UpdateProgress(ProgressStatus progress)
        {
            if (progressBarSkinDownload.InvokeRequired)
                progressBarSkinDownload.BeginInvoke(new Action(() => progressBarSkinDownload.Value = progress.Percentage));
            else
                progressBarSkinDownload.Value = progress.Percentage;

            var readableProgress = $"{GetReadableSize(progress.Processed)}/{GetReadableSize(progress.Total)} ({GetReadableSize(progress.Remaining)} remaining)";
            if (labelProgress.InvokeRequired)
                labelProgress.BeginInvoke(new Action(() => labelProgress.Text = readableProgress));
            else
                labelProgress.Text = readableProgress;

            if (labelPercentage.InvokeRequired)
                labelPercentage.BeginInvoke(new Action(() => labelPercentage.Text = $"{progress.Percentage}%"));
            else
                labelPercentage.Text = $"{progress.Percentage}%";


            var description = $"{progress.Description} ";
            if (label_Status.InvokeRequired)
                label_Status.BeginInvoke(new Action(() => label_Status.Text = description));
            else
                label_Status.Text = description;

        }

        private string GetReadableSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes = bytes / 1024;
            }

            return $"{bytes:0.##} {sizes[order]}";
        }
        private async void buttonExec_Click(object sender, EventArgs e)
        {
            if (!_diffs.Any() || listViewDiffs.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please selected the items that you would be download.");
                return;
            }

            var diffs = listViewDiffs.CheckedItems
                  .OfType<ListViewItem>()
                  .Select(x => _diffs[(int)x.Tag])
                  .ToList();

            labelPercentage.Visible = true;
            progressBarSkinDownload.Visible = true;
            labelProgress.Visible = true;

            await _diffManager.ExecuteDiff(diffs, UpdateProgress);
            await GetDiff();

            labelPercentage.Visible = false;
            labelProgress.Visible = false;
            progressBarSkinDownload.Visible = false;
        }


    }
}
