using WikiSync.Models;

namespace WikiSync.Interfaces
{
    public interface ISyncQueueService
    {
        bool Enqueue(FileChange fileChange);

        IAsyncEnumerable<FileChange> ReadAllAsync(
            CancellationToken cancellationToken);
    }
}
