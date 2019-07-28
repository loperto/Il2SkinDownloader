using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Il2.GreatBattles;
using Il2.RemoteDrive;
using Newtonsoft.Json;

namespace Il2SkinDownloader
{
    public partial class Form1 : Form
    {
        private Il2Game _il2;
        private const string SettingsFileName = "settings.json";
        private Configuration _configuration;
        private GoogleDrive _googleDrive;
        public Form1()
        {
            InitializeComponent();
            _configuration = GetCurrentConfiguration();
            if (!string.IsNullOrWhiteSpace(_configuration.Il2Path))
            {
                textBox_Il2Path.Text = _configuration.Il2Path;
                CheckSkinUpdates();
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

        private void CheckSkinUpdates()
        {
            _il2 = new Il2Game(_configuration.Il2Path);
            _googleDrive = new GoogleDrive("skinDownloader");
            _googleDrive.Connect("auth.json");
            var remoteFiles = _googleDrive.GetFiles();

            var localFiles = _il2.GetCustomSkinDirectories()
                .SelectMany(directoryInfo =>
                    Directory.EnumerateFiles(directoryInfo.FullName).
                        Select(file => new FileInfo(file))
                    );

        }

        private void Button_OpenIl2Folder_Click(object sender, EventArgs e)
        {
            var openFolderDialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = false,
                Description = $"Select the {Il2Game.Il2FolderName} folder path",
            };

            var result = openFolderDialog.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(openFolderDialog.SelectedPath))
            {
                var directoryInfo = new DirectoryInfo(openFolderDialog.SelectedPath);
                if (directoryInfo.Name != Il2Game.Il2FolderName)
                {
                    MessageBox.Show($"Selected path not valid. The folder must be {Il2Game.Il2FolderName}");
                }
                else
                {
                    textBox_Il2Path.Text = directoryInfo.FullName;
                    _configuration.Il2Path = directoryInfo.FullName;
                    _il2 = new Il2Game(_configuration.Il2Path);
                    SaveConfiguration(_configuration);
                    CheckSkinUpdates();
                }
            }
        }
    }

    public class Configuration
    {
        public string Il2Path { get; set; }
    }
}
