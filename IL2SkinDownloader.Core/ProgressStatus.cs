namespace IL2SkinDownloader.Core
{
    public class ProgressStatus
    {
        public long Total { get; internal set; }
        public long Processed { get; internal set; }
        public long Remaining => Total - Processed;
        public int Percentage => (int)(100 * (Processed + 1) / Total);
        public string Description { get; set; }
        public static ProgressStatus Create(long total, long processed, string description)
        {
            return new ProgressStatus
            {
                Total = total,
                Processed = processed,
                Description = description
            };
        }
    }
}