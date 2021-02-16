using System;
using System.IO;
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
        public async Task TestDiffGoogleDrive()
        {
            var googleDriveSkinDrive = new GoogleDriveSkinDrive();
            var diffManager = new DiffManager(googleDriveSkinDrive, @"G:\Steam\steamapps\common\IL-2 Sturmovik Battle of Stalingrad");
            var diffs = await diffManager.GetDiffAsync();
            await diffManager.ExecuteDiff(diffs);
        }

        [Test]
        public async Task TestDiffFtp()
        {
            var skinDrive = new FtpSkinDrive();
            var diffManager = new DiffManager(skinDrive, @"G:\Steam\steamapps\common\IL-2 Sturmovik Battle of Stalingrad");
            var diffs = await diffManager.GetDiffAsync();
            await diffManager.ExecuteDiff(diffs);
        }


        [Test]
        public async Task FolderFromUri()
        {
            var result = "/";
            var filePath = "/skindownloader/Yak7Bs36/Yak-7B Eux 86.dds";
            var lastIndex = filePath.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase);
            if (lastIndex != -1)
            {
                result = filePath.Substring(0,lastIndex);
            }

            Console.WriteLine(result);
        }


    }
}