using System;
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
        public async Task GoogleDriveGetAllFiles()
        {
            var googleDrive = new GoogleDriveWrapper();

            googleDrive.Connect("auth.json", "skinDownloader");
            var files = await googleDrive.GetFoldersAsync();

            foreach (var googleDriveItem in files)
            {
                await googleDrive.DeleteAsync(googleDriveItem.Id);
                Console.WriteLine(googleDriveItem);
            }
        }

        [Test]
        public async Task GoogleDriveInfos()
        {
            var googleDrive = new GoogleDriveWrapper();
            googleDrive.Connect("auth.json", "skinDownloader");
            await googleDrive.GetInfos();
        }

        [Test]
        public async Task ConnectWithServiceAccount()
        {
            var googleDrive = new GoogleDriveWrapper();
            googleDrive.Connect("auth.json", "skinDownloader");
            var folders = await googleDrive.GetFoldersAsync();
            foreach (var directory in folders)
            {
                Console.WriteLine(directory.Name);
                var files = await googleDrive.GetFilesInDirectoryAsync(directory.Id, CancellationToken.None);
                foreach (var file in files)
                {
                    Console.WriteLine(file.Name);
                }
            }
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
                result = filePath.Substring(0, lastIndex);
            }

            Console.WriteLine(result);
        }


    }
}