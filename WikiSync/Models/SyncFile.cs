namespace WikiSync.Models
{
    public class SyncFile
    {
        public string RelativePath { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
