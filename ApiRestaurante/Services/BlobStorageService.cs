using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using RestaurantesAPI.Interfaces;

namespace AvaliacaoRestaurantesAPI.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureBlobStorage:ConnectionString"]!;
            var containerName = configuration["AzureBlobStorage:ContainerName"]!;

            _containerClient = new BlobContainerClient(connectionString, containerName);
            _containerClient.CreateIfNotExists(PublicAccessType.Blob);
        }

        public async Task<string> UploadFotoPerfilAsync(string usuarioId, IFormFile arquivo)
        {
            var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
            var nomeBlob = $"perfil/{usuarioId}{extensao}";

            var blobClient = _containerClient.GetBlobClient(nomeBlob);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = arquivo.ContentType
            };

            await using var stream = arquivo.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });

            return blobClient.Uri.ToString();
        }

        public async Task DeletarFotoPerfilAsync(string urlFoto)
        {
            if (string.IsNullOrWhiteSpace(urlFoto))
                return;

            var uri = new Uri(urlFoto);
            var nomeBlob = uri.AbsolutePath.TrimStart('/').Replace($"{_containerClient.Name}/", string.Empty);

            var blobClient = _containerClient.GetBlobClient(nomeBlob);
            await blobClient.DeleteIfExistsAsync();
        }

        public async Task<List<string>> UploadFotosReviewAsync(string reviewId, IList<IFormFile> arquivos)
        {
            var urls = new List<string>();

            for (int i = 0; i < arquivos.Count; i++)
            {
                var arquivo = arquivos[i];
                var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
                var nomeBlob = $"reviews/{reviewId}/{i}{extensao}";

                var blobClient = _containerClient.GetBlobClient(nomeBlob);

                var blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = arquivo.ContentType
                };

                await using var stream = arquivo.OpenReadStream();
                await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });

                urls.Add(blobClient.Uri.ToString());
            }

            return urls;
        }

        public async Task DeletarFotosReviewAsync(IEnumerable<string?> urlFotos)
        {
            foreach (var urlFoto in urlFotos)
            {
                if (string.IsNullOrWhiteSpace(urlFoto))
                    continue;

                var uri = new Uri(urlFoto);
                var nomeBlob = uri.AbsolutePath.TrimStart('/').Replace($"{_containerClient.Name}/", string.Empty);

                var blobClient = _containerClient.GetBlobClient(nomeBlob);
                await blobClient.DeleteIfExistsAsync();
            }
        }
    }
}
