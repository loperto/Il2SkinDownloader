using System;

namespace IL2SkinDownloader.Core
{
    public class FileLocation
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime CreationDateTime { get; set; }
        public DateTime? LastUpdateDateTime { get; set; }
        public DateTime LastUpdate => LastUpdateDateTime ?? CreationDateTime;
        public long Size { get; set; }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Path)}: {Path}";
        }
    }
}