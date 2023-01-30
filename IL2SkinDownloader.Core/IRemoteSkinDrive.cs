using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentFTP;

namespace IL2SkinDownloader.Core
{
    public interface IRemoteSkinDrive
    {
        Task Connect();
        Task<IEnumerable<RemoteDirectory>> GetDirectories();
        Task<IEnumerable<FileLocation>> GetDirectoryFiles(string directoryName);
        Task DownloadFile(FileLocation fileLocation, string downloadPath, Action<long> onProgress = null);
    }
}