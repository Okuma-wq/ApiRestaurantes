namespace RestaurantesAPI.Interfaces
{
    public interface IBlobStorageService
    {
        Task<string> UploadFotoPerfilAsync(string usuarioId, IFormFile arquivo);
        Task DeletarFotoPerfilAsync(string urlFoto);
    }
}
