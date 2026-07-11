using WikiSync.Models;

namespace WikiSync.Interfaces
{
    public interface IStorageService
    {
        Task SaveAsync(
            SyncFile file,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            string relativePath,
            CancellationToken cancellationToken = default);

        Task RenameAsync(
            string oldRelativePath,
            string newRelativePath,
            CancellationToken cancellationToken = default);
    }
}
