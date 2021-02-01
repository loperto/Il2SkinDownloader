using System;
using System.Collections.Generic;

namespace IL2SkinDownloader.Core.GoogleDrive
{
    public class GoogleDriveDirectory
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class GoogleDriveItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DownloadLink { get; set; }
        public string BrowserViewLink { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public IList<string> Parents { get; set; }
        public long Size { get; set; }

        public GoogleDriveItem(string id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(CreatedTime)}: {CreatedTime}, {nameof(ModifiedTime)}: {ModifiedTime}, {nameof(Size)}: {Size}";
        }
    }
}