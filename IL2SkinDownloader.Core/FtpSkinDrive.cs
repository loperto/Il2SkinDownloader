using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;

namespace IL2SkinDownloader.Core
{
    public class FtpSkinDrive : IRemoteSkinDrive
    {
        private FtpClient _client;
        private Dictionary<string, IEnumerable<FileLocation>> _filesCache = null;
        private IReadOnlyList<RemoteDirectory> _directoriesCache;

        public Task Connect()
        {
            _client = new FtpClient("ftp://79.9.2.47", "skindownloader", "G3m1n1_ftp");
            _client.Connect();
            return Task.CompletedTask;
        }

        private FileLocation Convert(FtpListItem file)
        {
            return new FileLocation
            {
                Name = file.Name,
                Id = file.FullName,
                Size = file.Size,
                LastUpdateDateTime = file.Modified,
                CreationDateTime = file.Created,
                Path = file.FullName,
            };
        }

        private string GetParentPath(FtpListItem item)
        {
            var filePath = item.FullName;
            var result = "/";
            var lastIndex = filePath.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase);
            if (lastIndex != -1)
            {
                result = filePath.Substring(0, lastIndex);
            }

            return result;
        }

        public async Task<IEnumerable<RemoteDirectory>> GetDirectories()
        {
            if (_directoriesCache == null)
            {
                await RefreshLocalCache();
            }

            return _directoriesCache;
        }

        private Task RefreshLocalCache()
        {
            var list = _client.GetListing("/skindownloader", FtpListOption.Recursive);

            _filesCache ??= list.Where(x => x.Type == FtpObjectType.File)
                .GroupBy(GetParentPath)
                .Select(group => new { Dir = @group.Key, Files = @group.Select(Convert) })
                .ToDictionary(x => x.Dir, x => x.Files);

            _directoriesCache ??= list
                .Where(x => x.Type == FtpObjectType.Directory)
                .Select(x => new RemoteDirectory
                {
                    Name = x.Name,
                    Id = x.FullName,
                }).ToList();

            return Task.CompletedTask;
        }

        public async Task<IEnumerable<FileLocation>> GetDirectoryFiles(string directoryName)
        {
            if (_filesCache == null)
            {
                await RefreshLocalCache();
            }
            if (_filesCache.TryGetValue(directoryName, out var fromDictionary))
                return fromDictionary;
            return Enumerable.Empty<FileLocation>();
        }

        public Task DownloadFile(FileLocation fileLocation, string downloadPath, Action<long> onProgress = null)
        {
            _client.DownloadFile(downloadPath, fileLocation.Path, FtpLocalExists.Overwrite, FtpVerify.None, f => onProgress?.Invoke(f.TransferredBytes));
            return Task.CompletedTask;
        }
    }
}