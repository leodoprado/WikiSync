using WikiSync.Models;

namespace WikiSync.Interfaces
{
    public interface ISyncService
    {
        Task<SyncFile> PrepareFileAsync(
            string filePath,
            CancellationToken cancellationToken = default);
    }
}
