using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IL2SkinDownloader.Core;
using IL2SkinDownloader.Core.GoogleDrive;
using NUnit.Framework;

namespace IL2SkinDownloader.Tests
{
    [Explicit]
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task GoogleDriveConnection()
        {
            var googleDrive = new GoogleDriveWrapper();
            googleDrive.Connect("auth.json", "skinDownloader");
            var folders = await googleDrive.GetFoldersAsync();
            var folder = folders.FirstOrDefault(x => x.Name == "A20B");
            var files = await googleDrive.GetFilesInDirectoryAsync(folder.Id, CancellationToken.None);
        }

        [Test]
        public async Task TestDiff()
        {
            var googleDriveSkinDrive = new GoogleDriveSkinDrive();
            var diffManager = new DiffManager(googleDriveSkinDrive, "C:\\Temp\\IL-2 Sturmovik Great Battles");
            var diffs = await diffManager.GetDiffAsync();
            await diffManager.ExecuteDiff(diffs);
        }
    }
}