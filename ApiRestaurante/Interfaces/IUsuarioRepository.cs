using API.Models;

namespace RestaurantesAPI.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<List<Usuario>> ObterTodosAsync();
        Task<Usuario?> ObterPorIdAsync(string id);
        Task<Usuario?> ObterPorEmailAsync(string email);
        Task AdicionarAsync(Usuario usuario);
        Task AtualizarAsync(Usuario usuario);
        Task RemoverAsync(string id);
        Task AdicionarAosFavoritosAsync(string idUsuario, string idRestaurante);
    }
}
