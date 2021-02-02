using System;

namespace IL2SkinDownloader.Core
{
    public class DiffInfo
    {
        public FileLocation Remote { get; }
        public FileLocation Local { get; }
        public string LocalDestination { get; }
        public string GroupId { get; }
        public Status Status { get; }

        public DiffInfo(FileLocation remote, FileLocation local, string localDestination, string groupId)
        {
            Remote = remote;
            Local = local;
            LocalDestination = localDestination;
            GroupId = groupId;
            if (Remote != null && Local == null)
                Status = Status.Added;
            else if (Remote != null && Local != null && (Remote.LastUpdateDateTime > Local.LastUpdateDateTime || Remote.Size != Local.Size))
                Status = Status.Updated;
            else if (Remote == null && Local != null)
                Status = Status.Deleted;
            else
                Status = Status.Installed;
        }
    }
}