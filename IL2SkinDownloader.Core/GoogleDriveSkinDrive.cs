using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IL2SkinDownloader.Core.GoogleDrive;

namespace IL2SkinDownloader.Core
{
    public class GoogleDriveSkinDrive : IRemoteSkinDrive
    {
        private readonly GoogleDriveWrapper _googleDriveWrapper;
        private Dictionary<string, IEnumerable<GoogleDriveItem>> _forDirectoryCache;

        public GoogleDriveSkinDrive()
        {
            _googleDriveWrapper = new GoogleDriveWrapper();
        }
        private FileLocation Convert(GoogleDriveItem googleDrive)
        {
            return new FileLocation
            {
                Id = googleDrive.Id,
                CreationDateTime = googleDrive.CreatedTime,
                LastUpdateDateTime = googleDrive.ModifiedTime,
                Name = googleDrive.Name,
                Path = googleDrive.DownloadLink,
                Size = googleDrive.Size,
            };
        }

        public Task Connect()
        {
            _googleDriveWrapper.Connect(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "auth.json"), "skinDownloader");
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<RemoteDirectory>> GetDirectoriesAsync()
        {
            var directories = await _googleDriveWrapper.GetFoldersAsync();
            return directories.Select(x => new RemoteDirectory { Id = x.Id, Name = x.Name });
        }

        public async Task<IEnumerable<FileLocation>> GetDirectoryFilesAsync(string directoryName)
        {
            if (_forDirectoryCache == null)
            {
                var fromServer = await _googleDriveWrapper.GetFilesAsync(CancellationToken.None);
                _forDirectoryCache = fromServer.SelectMany(x => x.Parents).Distinct().Select(directory =>
                        new { directory, files = fromServer.Where(x => x.Parents.Contains(directory)) })
                    .ToDictionary(x => x.directory, x => x.files);
            }

            if (_forDirectoryCache.TryGetValue(directoryName, out var fromDictionary))
                return fromDictionary.Select(Convert);
            return Enumerable.Empty<FileLocation>();
        }

        public Task DownloadFileAsync(FileLocation fileLocation, string downloadPath, Action<long> onProgress = null)
        {
            return _googleDriveWrapper.DownloadAsync(fileLocation.Id, downloadPath, onProgress);
        }
    }
}