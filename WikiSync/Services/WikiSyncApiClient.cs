using WikiSync.Contracts;
using WikiSync.Interfaces;
using WikiSync.Models;

namespace WikiSync.Services
{
    public class WikiSyncApiClient : IWikiSyncApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WikiSyncApiClient> _logger;

        public WikiSyncApiClient(HttpClient httpClient, ILogger<WikiSyncApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> UploadFileAsync(SyncFile syncFile, CancellationToken cancellationToken = default)
        {
            var request = new UploadFileRequest
            {
                RelativePath = syncFile.RelativePath,
                Content = syncFile.Content,
                Hash = syncFile.Hash,
                LastModifiedAt = syncFile.LastModified
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "api/sync/upload",
                    request,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "A API recusou o arquivo {RelativePath}. Status: {StatusCode}",
                        syncFile.RelativePath,
                        response.StatusCode);

                    return false;
                }

                var result =
                    await response.Content.ReadFromJsonAsync<UploadFileResponse>(
                        cancellationToken: cancellationToken);

                _logger.LogInformation(
                    "Resposta da API: {Message}",
                    result?.Message);

                return result?.Success == true;
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(
                    exception,
                    "Não foi possível acessar a API do WikiSync.");

                return false;
            }
        }
    }
}
