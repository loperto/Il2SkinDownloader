using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Mandrega.Common;

namespace Mandrega.Installer
{
    public static class Installer
    {
        public static async Task DownloadFileAsync(string remoteUri, string destinationPath)
        {
            using (var client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) })
            {
                var response = await client.GetAsync(new Uri(remoteUri));
                if (response.IsSuccessStatusCode)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    File.WriteAllBytes(destinationPath, bytes);
                }
            }
        }
        public static async Task Install(StaticConfiguration configuration)
        {
            var alreadyInstalled = IsAlreadyInstalled(configuration);
            if (alreadyInstalled) return;

            if (!Directory.Exists(configuration.UpdaterInstallFolder))
                Directory.CreateDirectory(configuration.UpdaterInstallFolder);
            if (!Directory.Exists(configuration.HostInstallFolder))
                Directory.CreateDirectory(configuration.HostInstallFolder);

            var inRunning = Process.GetProcessesByName(configuration.HostExeName.Replace(".exe", string.Empty));
            foreach (var process in inRunning)
            {
                process.Kill();
            }

            foreach (var directory in Directory.EnumerateDirectories(configuration.HostInstallFolder))
            {
                Directory.Delete(directory, true);
            }
            foreach (var file in Directory.EnumerateFiles(configuration.HostInstallFolder))
            {
                File.Delete(file);
            }

            await DownloadFileAsync(configuration.HostZipRemoteUrl, configuration.HostZipPath);
            ZipFile.ExtractToDirectory(configuration.HostZipPath, configuration.HostInstallFolder);

            var start = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = configuration.HostExePath,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            start.Start();
        }

        private static bool IsAlreadyInstalled(StaticConfiguration configuration)
        {
            return Directory.Exists(configuration.HostInstallFolder) && File.Exists(configuration.HostExePath);
        }
    }
}