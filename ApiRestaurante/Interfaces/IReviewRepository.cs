using AvaliacaoRestaurantesAPI.Models;

namespace RestaurantesAPI.Interfaces
{
    public interface IReviewRepository
    {
        Task<List<Review>> ObterPorRestauranteAsync(string idRestaurante);
        Task<List<Review>> ObterPorUsuarioAsync(string idUsuario);
        Task<Review?> ObterPorIdAsync(string id);
        Task AdicionarAsync(Review review);
        Task AtualizarAsync(Review review);
        Task RemoverAsync(string id);
    }
}
