using Microsoft.AspNetCore.Mvc;
using WikiSync.Contracts;

namespace WikiSync.Controllers
{
    [ApiController]
    [Route("api/sync")]
    public class SyncController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SyncController> _logger;

        public SyncController(
            IConfiguration configuration,
            ILogger<SyncController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<ActionResult<UploadFileResponse>> UploadAsync(
            UploadFileRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.RelativePath))
            {
                return BadRequest(new UploadFileResponse
                {
                    Success = false,
                    Message = "O caminho relativo é obrigatório."
                });
            }

            var storagePath =
                _configuration["WikiSync:StoragePath"]
                ?? throw new InvalidOperationException(
                    "O caminho de armazenamento não foi configurado.");

            var safeRelativePath = request.RelativePath
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);

            var destinationPath = Path.GetFullPath(
                Path.Combine(storagePath, safeRelativePath));

            var normalizedStoragePath = Path.GetFullPath(storagePath);

            if (!destinationPath.StartsWith(
                    normalizedStoragePath,
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new UploadFileResponse
                {
                    Success = false,
                    Message = "O caminho informado é inválido."
                });
            }

            var destinationDirectory =
                Path.GetDirectoryName(destinationPath);

            if (!string.IsNullOrWhiteSpace(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            await System.IO.File.WriteAllTextAsync(
                destinationPath,
                request.Content,
                cancellationToken);

            _logger.LogInformation(
                "Arquivo recebido e salvo em {DestinationPath}",
                destinationPath);

            return Ok(new UploadFileResponse
            {
                Success = true,
                Message = "Arquivo recebido e salvo com sucesso.",
                RelativePath = request.RelativePath
            });
        }
    }
}