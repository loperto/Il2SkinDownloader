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
        public GoogleDriveSkinDrive()
        {
            _googleDriveWrapper = new GoogleDriveWrapper("skinDownloader");
            _googleDriveWrapper.Connect(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "auth.json"));
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
            };
        }



        public async Task<IEnumerable<RemoteDirectory>> GetDirectoriesAsync()
        {
            var directories = await _googleDriveWrapper.GetFoldersAsync();
            return directories.Select(x => new RemoteDirectory { Id = x.Id, Name = x.Name });
        }

        public async Task<IEnumerable<FileLocation>> GetDirectoryFilesAsync(string directoryName)
        {
            var files = await _googleDriveWrapper.GetFilesInDirectoryAsync(directoryName, CancellationToken.None);
            return files.Select(Convert);
        }

        public Task DownloadFileAsync(FileLocation fileLocation, string downloadPath)
        {
            return _googleDriveWrapper.DownloadAsync(fileLocation.Id, downloadPath);
        }
    }
}