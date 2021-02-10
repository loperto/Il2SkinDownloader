using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentFTP;

namespace IL2SkinDownloader.Core
{
    public class FtpSkinDrive : IRemoteSkinDrive
    {
        private FtpClient _client;
        public Task Connect()
        {
            _client = new FtpClient("ftp://79.9.2.47", new NetworkCredential("skindownloader", "G3m1n1_ftp"));
            return _client.ConnectAsync();
        }

        public async Task<IEnumerable<RemoteDirectory>> GetDirectoriesAsync()
        {
            var list = await _client.GetListingAsync();
            return list.Where(x => x.Type == FtpFileSystemObjectType.Directory).Select(x => new RemoteDirectory
            {
                Name = x.Name,
                Id = x.FullName,
            });

        }

        public Task<IEnumerable<FileLocation>> GetDirectoryFilesAsync(string directoryName)
        {
            throw new NotImplementedException();
        }

        public Task DownloadFileAsync(FileLocation fileLocation, string downloadPath, Action<long> onProgress = null)
        {
            throw new NotImplementedException();
        }
    }
}