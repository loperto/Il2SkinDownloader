using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Il2SkinDownloader.GoogleDrive;
using Il2SkinDownloader.IL2;

namespace Il2SkinDownloader
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

        public IEnumerable<Folder> GetRemoteDirectories()
        {
            return _googleDriveWrapper.GetFolders()
                .Select(x => new Folder { Id = x.Id, Name = x.Name });
        }

        private List<(Item, Status)> GetDiff()
        {
            //var configuration = StaticConfiguration.GetCurrentConfiguration();
            //Installer.Install(configuration);
            var il2LocalSkinsRootPath = IL2Helpers.SkinDirectoryPath(_il2InstallPath);
            var remoteFolders = GetRemoteDirectories();

            var result = new List<(Item, Status)>();
            foreach (var remoteFolder in remoteFolders)
            {
                var localFolderPath = Path.Combine(il2LocalSkinsRootPath, remoteFolder.Name);

                var remoteFileForFolder = remoteFiles
                    .Where(x => x.Parents == remoteFolder.Id)
                    .ToArray();

                var localFilesForFolder = Directory.EnumerateFiles(localFolderPath)
                    .Select(file => new FileInfo(file))
                    .ToArray();

                foreach (var remoteFile in remoteFileForFolder)
                {
                    var existingFile = localFilesForFolder.FirstOrDefault(l => string.Equals(remoteFile.Name, l.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (existingFile == null)
                    {
                        result.Add((Convert(remoteFile), Status.Added));
                    }
                    else if (remoteFile.ModifiedTime > existingFile.LastWriteTime)
                    {
                        result.Add((Convert(remoteFile), Status.Updated));
                    }
                }

                var toDel = localFilesForFolder.Where(localFile => !remoteFileForFolder.Select(x => x.Name)
                        .Contains(localFile.Name, StringComparer.InvariantCultureIgnoreCase))
                    .Select(x => (Convert(x), Status.Deleted));

                result.AddRange(toDel);
            }

            return result;
        }

    }
}