using WikiSync.Models;

namespace WikiSync.Interfaces
{
    public interface IWikiSyncApiClient
    {
        Task<bool> UploadFileAsync(
            SyncFile syncFile,
            CancellationToken cancellationToken = default);
    }
}
