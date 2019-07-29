using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Il2.GreatBattles;
using Il2.RemoteDrive;
using NUnit.Framework;

namespace Il2.SkinDownloader.Tests
{
    public class GoogleDriveTests
    {
        private GoogleDrive _drive;
        [SetUp]
        public void Setup()
        {
            _drive = new GoogleDrive("skinDownloader");
            _drive.Connect(@"C:\Temp\auth.json");
        }

        [Test]
        public async Task CreateMasterFolder()
        {
            await _drive.CreateFolder("Il2SkinDownloader");
        }

        [Test]
        public async Task GetFiles()
        {
            var files = await _drive.GetFilesAsync(CancellationToken.None);
            foreach (var googleDriveItem in files)
            {
                Console.WriteLine(googleDriveItem);
            }
        }

        [Test]
        public async Task ShareItem()
        {
            await _drive.Share("1XoAGf_iPTJ5zzW-mLwyoTHy-GVD2TJ0n", "guest-262@skindownloader.iam.gserviceaccount.com");
        }

        [Test]
        public async Task CreatePlaneSkinFolders()
        {
            var il2 = new Il2Game(@"Z:\Steam\steamapps\common\IL-2 Sturmovik Battle of Stalingrad");
            var planes = il2.GetCustomSkinDirectories();
            foreach (var plane in planes)
            {
                Console.WriteLine(plane.Name);
                await _drive.CreateFolder(plane.Name, "1XoAGf_iPTJ5zzW-mLwyoTHy-GVD2TJ0n");
            }
        }

        [Test]
        public async Task DownloadFile()
        {
            await _drive.DownloadAsync("1Yk19hsN9lOIPXI3GQmwbto1KN1B28yeR", "C:\\Temp\\fw190-loperto.dds");
        }

        [Test]
        public async Task CreateFakeIl2Folder()
        {
            var il2FakePath = "C:\\Temp\\IL-2 Sturmovik Battle of Stalingrad";
            var completePath = Path.Combine(il2FakePath, "data", "graphics", "skins");
            Directory.CreateDirectory(completePath);
            var files = await _drive.GetFilesAsync(CancellationToken.None);
            foreach (var directory in files.Where(x => x.IsFolder))
            {
                Directory.CreateDirectory(Path.Combine(completePath, directory.Name));
            }
        }
    }
}