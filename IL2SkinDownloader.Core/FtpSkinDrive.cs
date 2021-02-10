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
        private Dictionary<string, IEnumerable<FileLocation>> _cache = null;
        public Task Connect()
        {
            _client = new FtpClient("ftp://79.9.2.47", new NetworkCredential("skindownloader", "G3m1n1_ftp"));
            return _client.ConnectAsync();
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

        public async Task<IEnumerable<RemoteDirectory>> GetDirectoriesAsync()
        {
            var list = await _client.GetListingAsync("/skindownloader", FtpListOption.Recursive);
            _cache ??= list.Where(x => x.Type == FtpFileSystemObjectType.File)
                .GroupBy(GetParentPath)
                .Select(group => new { Dir = @group.Key, Files = @group.Select(Convert) })
                .ToDictionary(x => x.Dir, x => x.Files);

            return list
                .Where(x => x.Type == FtpFileSystemObjectType.Directory)
                .Select(x => new RemoteDirectory
                {
                    Name = x.Name,
                    Id = x.FullName,
                });
        }

        public Task<IEnumerable<FileLocation>> GetDirectoryFilesAsync(string directoryName)
        {
            if (_cache.TryGetValue(directoryName, out var fromDictionary))
                return Task.FromResult(fromDictionary);
            return Task.FromResult(Enumerable.Empty<FileLocation>());
        }

        public class FtpDownloadProgress : IProgress<FtpProgress>
        {
            private readonly Action<long> _onProgress;

            public FtpDownloadProgress(Action<long> onProgress)
            {
                _onProgress = onProgress;
            }
            public void Report(FtpProgress value)
            {
                _onProgress.Invoke(value.TransferredBytes);
            }
        }

        public Task DownloadFileAsync(FileLocation fileLocation, string downloadPath, Action<long> onProgress = null)
        {
            var progress = onProgress != null ? new FtpDownloadProgress(onProgress) : null;
            return _client.DownloadAsync(fileLocation.Path, 0, progress, CancellationToken.None);
        }
    }
}