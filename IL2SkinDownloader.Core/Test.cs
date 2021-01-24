using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IL2SkinDownloader.Core.GoogleDrive;
using IL2SkinDownloader.Core.IL2;

namespace IL2SkinDownloader.Core
{
    public enum Status
    {
        Added,
        Updated,
        Deleted
    }

    public class Folder
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class Item
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime CreationDateTime { get; set; }
        public DateTime? LastUpdateDateTime { get; set; }
    }
    public class Test
    {
        private readonly string _il2InstallPath;
        private readonly GoogleDriveWrapper _googleDriveWrapper;
        public Test(string il2InstallPath)
        {
            _il2InstallPath = il2InstallPath;
            _googleDriveWrapper = new GoogleDriveWrapper("skinDownloader");
            _googleDriveWrapper.Connect(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "auth.json"));
        }
        private Item Convert(GoogleDriveItem googleDrive)
        {
            return new Item
            {
                Id = googleDrive.Id,
                CreationDateTime = googleDrive.CreatedTime,
                LastUpdateDateTime = googleDrive.ModifiedTime,
                Name = googleDrive.Name,
                Path = googleDrive.DownloadLink,
            };
        }

        private Item Convert(FileInfo localFile)
        {
            return new Item
            {
                Id = localFile.FullName,
                CreationDateTime = localFile.CreationTime,
                LastUpdateDateTime = localFile.LastWriteTime,
                Name = localFile.Name,
                Path = localFile.FullName,
            };
        }

        public async Task<IEnumerable<Folder>> GetRemoteDirectoriesAsync()
        {
            var directories = await _googleDriveWrapper.GetFoldersAsync();
            return directories.Select(x => new Folder { Id = x.Id, Name = x.Name });
        }

        public async Task<IEnumerable<Item>> GetRemoteFilesAsync(string folderName)
        {
            var files = await _googleDriveWrapper.GetFilesInDirectoryAsync(folderName, CancellationToken.None);
            return files.Select(Convert);
        }

        public IEnumerable<Item> GetLocalFiles(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return Enumerable.Empty<Item>();

            return Directory.EnumerateFiles(folderPath)
                .Select(file => new FileInfo(file))
                .Select(Convert);
        }

        public async Task<List<(Item, Status)>> GetDiff(bool deleteFileDoNotExistInDrive = false)
        {
            //var configuration = StaticConfiguration.GetCurrentConfiguration();
            //Installer.Install(configuration);
            var il2LocalSkinsRootPath = IL2Helpers.SkinDirectoryPath(_il2InstallPath);
            var remoteFolders = await GetRemoteDirectoriesAsync();

            var result = new List<(Item, Status)>();
            foreach (var remoteFolder in remoteFolders)
            {
                var localFolderPath = Path.Combine(il2LocalSkinsRootPath, remoteFolder.Name);
                var remoteFileForFolder = (await GetRemoteFilesAsync(remoteFolder.Id)).ToArray();
                var localFilesForFolder = GetLocalFiles(localFolderPath).ToArray();

                foreach (var remoteFile in remoteFileForFolder)
                {
                    var existingFile = localFilesForFolder.FirstOrDefault(l => string.Equals(remoteFile.Name, l.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (existingFile == null)
                    {
                        result.Add((remoteFile, Status.Added));
                    }
                    else if (remoteFile.LastUpdateDateTime > existingFile.LastUpdateDateTime)
                    {
                        result.Add((remoteFile, Status.Updated));
                    }
                }

                if (deleteFileDoNotExistInDrive)
                {
                    var toDel = localFilesForFolder.Where(localFile => !remoteFileForFolder.Select(x => x.Name)
                            .Contains(localFile.Name, StringComparer.InvariantCultureIgnoreCase))
                        .Select(x => (x, Status.Deleted));

                    result.AddRange(toDel);
                }
            }

            return result;
        }

    }
}