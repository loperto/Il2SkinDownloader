﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IL2SkinDownloader.Core.IL2;

namespace IL2SkinDownloader.Core
{
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
                Size = localFile.Length,
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

        public async Task ExecuteDiff(List<DiffInfo> diffs, Action<ProgressStatus> onProgress = null)
        {
            var totalSize = diffs.Sum(x => x.Remote.Size);
            var downloaded = 0L;
            onProgress?.Invoke(ProgressStatus.Create(totalSize, downloaded, "Starting downloading..."));
            foreach (var diff in diffs)
            {
                var currentFileDownloaded = 0L;
                switch (diff.Status)
                {
                    case Status.Added:
                        if (!Directory.Exists(diff.LocalDestination))
                            Directory.CreateDirectory(diff.LocalDestination);
                        await _remoteSkinDrive.DownloadFile(diff.Remote, Path.Combine(diff.LocalDestination, diff.Remote.Name),
                            bytes =>
                            {
                                currentFileDownloaded = bytes;
                                var progress = ProgressStatus.Create(totalSize, currentFileDownloaded + downloaded, $"Downloading {diff.Remote.Name}");
                                onProgress?.Invoke(progress);
                            });
                        break;
                    case Status.Updated:
                        File.Delete(diff.Local.Path);
                        await _remoteSkinDrive. DownloadFile(diff.Remote, diff.Local.Path, bytes =>
                        {
                            currentFileDownloaded = bytes;
                            var progress = ProgressStatus.Create(totalSize, currentFileDownloaded + downloaded, $"Downloading {diff.Remote.Name}");
                            onProgress?.Invoke(progress);
                        });
                        break;
                    case Status.Deleted:
                        File.Delete(diff.Local.Path);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                downloaded += currentFileDownloaded;
            }
        }


        public async Task<List<DiffInfo>> GetDiffAsync(bool deleteFileDoNotExistInDrive = false)
        {
            //var configuration = StaticConfiguration.GetCurrentConfiguration();
            //Installer.Install(configuration);

            await _remoteSkinDrive.Connect();
            var il2LocalSkinsRootPath = IL2Helpers.SkinDirectoryPath(_il2InstallPath);
            var remoteFolders = (await _remoteSkinDrive.GetDirectories()).ToArray();

            var result = new List<DiffInfo>();
            foreach (var remoteFolder in remoteFolders)
            {
                var localFolderPath = Path.Combine(il2LocalSkinsRootPath, remoteFolder.Name);
                var remoteFileForFolder = (await _remoteSkinDrive.GetDirectoryFiles(remoteFolder.Id)).ToArray();
                var localFilesForFolder = GetLocalFiles(localFolderPath).ToArray();

                foreach (var remoteFile in remoteFileForFolder)
                {
                    var existingFile = localFilesForFolder.FirstOrDefault(l =>
                        string.Equals(remoteFile.Name, l.Name, StringComparison.InvariantCultureIgnoreCase));
                    
                    result.Add(new DiffInfo(remoteFile, existingFile, localFolderPath, remoteFolder.Name));
                }

                if (deleteFileDoNotExistInDrive)
                {
                    var toDel = localFilesForFolder.Where(localFile => !remoteFileForFolder.Select(x => x.Name)
                            .Contains(localFile.Name, StringComparer.InvariantCultureIgnoreCase))
                        .Select(x => new DiffInfo(null, x, x.Path, x.Name));

                    result.AddRange(toDel);
                }
            }

            return result.Where(x=> x.Status != Status.Installed).ToList();
        }

    }
}