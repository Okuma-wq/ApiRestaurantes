namespace RestaurantesAPI.Interfaces
{
    public interface IBlobStorageService
    {
        Task<string> UploadFotoPerfilAsync(string usuarioId, IFormFile arquivo);
        Task DeletarFotoPerfilAsync(string urlFoto);
        Task<List<string>> UploadFotosReviewAsync(string reviewId, IList<IFormFile> arquivos);
        Task DeletarFotosReviewAsync(IEnumerable<string?> urlFotos);
    }
}
