using WikiSync.Interfaces;
using WikiSync.Models;

namespace WikiSync.Services
{
    public class SyncService : ISyncService
    {
        private readonly IHashService _hashService;
        private readonly string _vaultPath;

        public SyncService(IHashService hashService, IConfiguration configuration)
        {
            _hashService = hashService;
            _vaultPath = configuration["WikiSync:VaultPath"]
                         ?? throw new InvalidOperationException("Vault path is not configured.");
        }

        public async Task<SyncFile> PrepareFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Arquivo não encontrado.", filePath);
            }

            var content = await File.ReadAllTextAsync(filePath, cancellationToken);

            var hash = _hashService.GenerateHash(filePath);

            var relativePath = Path.GetRelativePath(_vaultPath, filePath);

            var fileInfo = new FileInfo(filePath);

            return new SyncFile
            {
                FullPath = filePath,
                RelativePath = relativePath,
                Content = content,
                Hash = hash,
                LastModified = fileInfo.LastWriteTimeUtc
            };
        }
    }
}
