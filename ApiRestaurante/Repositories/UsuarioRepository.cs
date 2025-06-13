using API.Models;
using MongoDB.Driver;
using RestaurantesAPI.Interfaces;

namespace RestaurantesAPI.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly IMongoCollection<Usuario> _usuarios;

        public UsuarioRepository(IMongoDatabase database)
        {
            _usuarios = database.GetCollection<Usuario>("usuarios");
        }

        public async Task<List<Usuario>> ObterTodosAsync()
        {
            return await _usuarios.Find(_ => true).ToListAsync();
        }

        public async Task<Usuario?> ObterPorIdAsync(string id)
        {
            return await _usuarios.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Usuario?> ObterPorEmailAsync(string email)
        {
            return await _usuarios.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task AdicionarAsync(Usuario usuario)
        {
            await _usuarios.InsertOneAsync(usuario);
        }

        public async Task AtualizarAsync(Usuario usuario)
        {
            await _usuarios.ReplaceOneAsync(u => u.Id == usuario.Id, usuario);
        }

        public async Task RemoverAsync(string id)
        {
            await _usuarios.DeleteOneAsync(u => u.Id == id);
        }

        public async Task AdicionarAosFavoritosAsync(string idUsuario, string idRestaurante)
        {
            var update = Builders<Usuario>.Update.AddToSet(u => u.Favoritos, idRestaurante);
            await _usuarios.UpdateOneAsync(u => u.Id == idUsuario, update);
        }

    }
}
