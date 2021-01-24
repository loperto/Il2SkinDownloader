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
            var googleDrive = new GoogleDriveWrapper("skinDownloader");
            googleDrive.Connect("auth.json");
            var folders = await googleDrive.GetFoldersAsync();
            var folder = folders.FirstOrDefault(x => x.Name == "A20B");
            var files = await googleDrive.GetFilesInDirectoryAsync(folder.Id, CancellationToken.None);
        }

        [Test]
        public void TestDiff()
        {
            var diff = new Test(@"G:\Steam\steamapps\common\IL-2 Sturmovik Battle of Stalingrad");
            var result = diff.GetDiff();
        }
    }
}