using System.Collections.Generic;
using System.Threading.Tasks;

namespace IL2SkinDownloader.Core
{
    public interface IRemoteSkinDrive
    {
        Task<IEnumerable<RemoteDirectory>> GetDirectoriesAsync();
        Task<IEnumerable<FileLocation>> GetDirectoryFilesAsync(string directoryName);
        Task DownloadFileAsync(FileLocation fileLocation, string downloadPath);
    }
}