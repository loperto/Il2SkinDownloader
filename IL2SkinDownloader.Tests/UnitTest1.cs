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
        public async Task UploadAllFromLocal()
        {
            var geminiSquadrigliaService = new GoogleDriveWrapper();
            await geminiSquadrigliaService.Connect("558704167850-uf9fbin4vmlfdom7qjomiqrf14u4t1cc.apps.googleusercontent.com", "c04c8g7cV_qHr1oACg74aD3Z", "skinDownloader", CancellationToken.None);
            var remoteFiles = await geminiSquadrigliaService.GetFilesAsync(CancellationToken.None);
            var files = remoteFiles.Where(x => x.Name.EndsWith(".dds")).ToDictionary(x => x.Name);
            var remoteFolders = await geminiSquadrigliaService.GetFoldersAsync();
            var folders = remoteFolders.ToDictionary(x => x.Name);

            foreach (var directory in Directory.EnumerateDirectories(@"G:\Steam\steamapps\common\IL-2 Sturmovik Battle of Stalingrad\data\graphics\skins"))
            {
                var directoryInfo = new DirectoryInfo(directory);
                if (!folders.TryGetValue(directoryInfo.Name, out var remoteFolder))
                {
                    remoteFolder = await geminiSquadrigliaService.CreateFolderAsync(directoryInfo.Name, "1039aymF0KRTIrzx2cNF9dbYmb6XmP9w5");
                    folders.Add(remoteFolder.Name, remoteFolder);
                }

                foreach (var filePath in Directory.EnumerateFiles(directory))
                {
                    var fileInfo = new FileInfo(filePath);
                    if (!files.TryGetValue(fileInfo.Name, out var remoteFile))
                    {
                        Console.WriteLine($"Uploading {filePath}");
                        remoteFile = await geminiSquadrigliaService.UploadAsync(filePath, remoteFolder.Id);
                        files.Add(remoteFile.Name, remoteFile);
                        Console.WriteLine($"Upload completed {filePath}");
                    }
                }
            }
        }

        [Test]
        public async Task ConnectWithGeminiSquadriglia()
        {
            var geminiSquadrigliaService = new GoogleDriveWrapper();
            await geminiSquadrigliaService.Connect("558704167850-uf9fbin4vmlfdom7qjomiqrf14u4t1cc.apps.googleusercontent.com", "c04c8g7cV_qHr1oACg74aD3Z", "skinDownloader", CancellationToken.None);
            var files = await geminiSquadrigliaService.GetFilesAsync(CancellationToken.None);
            foreach (var googleDriveItem in files)
            {
                Console.WriteLine($"{googleDriveItem.Id} {googleDriveItem.Name}");
            }
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
                result = filePath.Substring(0, lastIndex);
            }

            Console.WriteLine(result);
        }


    }
}