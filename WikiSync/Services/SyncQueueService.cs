using System.Threading.Channels;
using WikiSync.Interfaces;
using WikiSync.Models;

namespace WikiSync.Services
{
    public class SyncQueueService : ISyncQueueService
    {
        private readonly Channel<FileChange> _queue;

        public SyncQueueService()
        {
            _queue = Channel.CreateUnbounded<FileChange>(
                new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false
                });
        }

        public bool Enqueue(FileChange fileChange)
        {
            return _queue.Writer.TryWrite(fileChange);
        }

        public IAsyncEnumerable<FileChange> ReadAllAsync(
            CancellationToken cancellationToken)
        {
            return _queue.Reader.ReadAllAsync(cancellationToken);
        }
    }
}
