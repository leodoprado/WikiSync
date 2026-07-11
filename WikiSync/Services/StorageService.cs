using WikiSync.Interfaces;
using WikiSync.Models;

namespace WikiSync.Services
{
    public class StorageService : IStorageService
    {
        private readonly string _storagePath;
        private readonly ILogger<StorageService> _logger;

        public StorageService(
            IConfiguration configuration,
            ILogger<StorageService> logger)
        {
            _storagePath =
                configuration["WikiSync:StoragePath"]
                ?? throw new InvalidOperationException(
                    "O caminho de armazenamento não foi configurado.");

            _logger = logger;
        }

        public async Task SaveAsync(
            SyncFile file,
            CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));

            if (string.IsNullOrWhiteSpace(file.RelativePath))
                throw new ArgumentException(
                    "O caminho relativo do arquivo é obrigatório.",
                    nameof(file));

            var safeRelativePath = file.RelativePath
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);

            var normalizedStoragePath =
                Path.GetFullPath(_storagePath);

            var destinationPath =
                Path.GetFullPath(
                    Path.Combine(
                        normalizedStoragePath,
                        safeRelativePath));

            if (!destinationPath.StartsWith(
                    normalizedStoragePath,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "O caminho do arquivo é inválido.");
            }

            var destinationDirectory =
                Path.GetDirectoryName(destinationPath);

            if (!string.IsNullOrWhiteSpace(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            await File.WriteAllTextAsync(
                destinationPath,
                file.Content,
                cancellationToken);

            _logger.LogInformation(
                "Arquivo salvo em {DestinationPath}",
                destinationPath);
        }

        public Task DeleteAsync(
    string relativePath,
    CancellationToken cancellationToken = default)
        {
            var destinationPath = GetSafeDestinationPath(relativePath);

            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);

                _logger.LogInformation(
                    "Arquivo removido do storage: {Path}",
                    destinationPath);
            }

            return Task.CompletedTask;
        }

        public Task RenameAsync(
            string oldRelativePath,
            string newRelativePath,
            CancellationToken cancellationToken = default)
        {
            var oldPath = GetSafeDestinationPath(oldRelativePath);
            var newPath = GetSafeDestinationPath(newRelativePath);

            if (!File.Exists(oldPath))
                return Task.CompletedTask;

            var directory = Path.GetDirectoryName(newPath);

            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            if (File.Exists(newPath))
                File.Delete(newPath);

            File.Move(oldPath, newPath);

            _logger.LogInformation(
                "Arquivo renomeado no storage: {OldPath} para {NewPath}",
                oldPath,
                newPath);

            return Task.CompletedTask;
        }

        private string GetSafeDestinationPath(string relativePath)
        {
            var safeRelativePath = relativePath
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);

            var normalizedStoragePath = Path.GetFullPath(_storagePath);

            var destinationPath = Path.GetFullPath(
                Path.Combine(normalizedStoragePath, safeRelativePath));

            var storagePathWithSeparator =
                normalizedStoragePath.TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            if (!destinationPath.StartsWith(
                    storagePathWithSeparator,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "O caminho do arquivo é inválido.");
            }

            return destinationPath;
        }
    }
}
