using WikiSync.Interfaces;
using WikiSync.Models;

namespace WikiSync.BackgroundServices
{
    public class SyncWorker : BackgroundService
    {
        private readonly ISyncQueueService _syncQueue;
        private readonly ISyncService _syncService;
        private readonly IWikiSyncApiClient _apiClient;
        private readonly ILogger<SyncWorker> _logger;
        private readonly IStorageService _storageService;
        private readonly string _vaultPath;

        public SyncWorker(
            ISyncQueueService syncQueue,
            ISyncService syncService,
            IWikiSyncApiClient apiClient,
            IStorageService storageService,
            IConfiguration configuration,
            ILogger<SyncWorker> logger)

        {
            _syncQueue = syncQueue;
            _syncService = syncService;
            _apiClient = apiClient;
            _storageService = storageService;
            _logger = logger;

            _vaultPath = configuration["WikiSync:VaultPath"]
                         ?? throw new InvalidOperationException("O caminho do Vault não foi configurado.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (
                var fileChange in _syncQueue.ReadAllAsync(stoppingToken))
            {
                await ProcessFileAsync(fileChange, stoppingToken);
            }
        }

        private async Task ProcessFileAsync(FileChange fileChange, CancellationToken cancellationToken)
        {
            try
            {
                switch (fileChange.ChangeType)
                {
                    case FileChangeType.Created:
                    case FileChangeType.Updated:
                        {
                            if (!File.Exists(fileChange.FilePath))
                                return;

                            var syncFile =
                                await _syncService.PrepareFileAsync(
                                    fileChange.FilePath,
                                    cancellationToken);

                            await _storageService.SaveAsync(
                                syncFile,
                                cancellationToken);

                            break;
                        }

                    case FileChangeType.Deleted:
                        {
                            var relativePath = Path.GetRelativePath(
                                _vaultPath,
                                fileChange.FilePath);

                            await _storageService.DeleteAsync(
                                relativePath,
                                cancellationToken);

                            break;
                        }

                    case FileChangeType.Renamed:
                        {
                            if (string.IsNullOrWhiteSpace(fileChange.OldFilePath))
                                return;

                            var oldRelativePath = Path.GetRelativePath(
                                _vaultPath,
                                fileChange.OldFilePath);

                            var newRelativePath = Path.GetRelativePath(
                                _vaultPath,
                                fileChange.FilePath);

                            await _storageService.RenameAsync(
                                oldRelativePath,
                                newRelativePath,
                                cancellationToken);

                            break;
                        }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Erro ao processar alteração de arquivo.");
            }
        }
    }
}