namespace WikiSync.Contracts
{
    public class UploadFileResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public string RelativePath { get; set; } = string.Empty;
    }
}
