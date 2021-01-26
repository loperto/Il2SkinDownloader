using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IL2SkinDownloader.Core.IL2;


namespace IL2SkinDownloader.Core
{
    public enum Status
    {
        Added,
        Updated,
        Deleted,
    }
    public class DiffManager
    {
        private readonly IRemoteSkinDrive _remoteSkinDrive;
        private readonly string _il2InstallPath;
        public DiffManager(IRemoteSkinDrive remoteSkinDrive, string il2InstallPath)
        {
            _remoteSkinDrive = remoteSkinDrive;
            _il2InstallPath = il2InstallPath;
        }
        private FileLocation Convert(FileInfo localFile)
        {
            return new FileLocation
            {
                Id = localFile.FullName,
                CreationDateTime = localFile.CreationTime,
                LastUpdateDateTime = localFile.LastWriteTime,
                Name = localFile.Name,
                Path = localFile.FullName,
            };
        }

        public IEnumerable<FileLocation> GetLocalFiles(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return Enumerable.Empty<FileLocation>();

            return Directory.EnumerateFiles(folderPath)
                .Select(file => new FileInfo(file))
                .Select(Convert);
        }

        public async Task ExecuteDiff(List<DiffInfo> diffs)
        {
            foreach (var diff in diffs)
            {
                var status = diff.GetStatus();
                switch (status)
                {
                    case Status.Added:
                        if (!Directory.Exists(diff.LocalDestination))
                            Directory.CreateDirectory(diff.LocalDestination);
                        await _remoteSkinDrive.DownloadFileAsync(diff.Remote, Path.Combine(diff.LocalDestination, diff.Remote.Name));
                        break;
                    case Status.Updated:
                        File.Delete(diff.Local.Path);
                        await _remoteSkinDrive.DownloadFileAsync(diff.Remote, Path.Combine(diff.Local.Path, diff.Local.Name));
                        break;
                    case Status.Deleted:
                        File.Delete(diff.Local.Path);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public class DiffInfo
        {
            public FileLocation Remote { get; }
            public FileLocation Local { get; }
            public string LocalDestination { get; }
            public DiffInfo(FileLocation remote, FileLocation local, string localDestination)
            {
                Remote = remote;
                Local = local;
                LocalDestination = localDestination;
            }
            public Status GetStatus()
            {
                if (Remote != null && Local == null)
                    return Status.Added;
                if (Remote != null && Local != null && Remote.LastUpdateDateTime > Local.LastUpdateDateTime)
                    return Status.Updated;
                if (Remote == null && Local != null)
                    return Status.Deleted;

                throw new NotSupportedException($"Invalid diff status Local:{Local} Remote:{Remote}");
            }
        }

        public async Task<List<DiffInfo>> GetDiffAsync(bool deleteFileDoNotExistInDrive = false)
        {
            //var configuration = StaticConfiguration.GetCurrentConfiguration();
            //Installer.Install(configuration);
            var il2LocalSkinsRootPath = IL2Helpers.SkinDirectoryPath(_il2InstallPath);
            var remoteFolders = await _remoteSkinDrive.GetDirectoriesAsync();

            var result = new List<DiffInfo>();
            foreach (var remoteFolder in remoteFolders)
            {
                var localFolderPath = Path.Combine(il2LocalSkinsRootPath, remoteFolder.Name);
                var remoteFileForFolder = (await _remoteSkinDrive.GetDirectoryFilesAsync(remoteFolder.Id)).ToArray();
                var localFilesForFolder = GetLocalFiles(localFolderPath).ToArray();

                foreach (var remoteFile in remoteFileForFolder)
                {
                    var existingFile = localFilesForFolder.FirstOrDefault(l => string.Equals(remoteFile.Name, l.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (existingFile == null)
                    {
                        result.Add(new DiffInfo(remoteFile, null, localFolderPath));
                    }
                    else if (remoteFile.LastUpdateDateTime > existingFile.LastUpdateDateTime)
                    {
                        result.Add(new DiffInfo(remoteFile, existingFile, localFolderPath));
                    }
                }

                if (deleteFileDoNotExistInDrive)
                {
                    var toDel = localFilesForFolder.Where(localFile => !remoteFileForFolder.Select(x => x.Name)
                            .Contains(localFile.Name, StringComparer.InvariantCultureIgnoreCase))
                        .Select(x => new DiffInfo(null, x, x.Path));

                    result.AddRange(toDel);
                }
            }

            return result;
        }

    }
}