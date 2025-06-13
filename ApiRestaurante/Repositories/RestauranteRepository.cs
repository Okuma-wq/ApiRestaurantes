using AvaliacaoRestaurantesAPI.Models;
using MongoDB.Driver;

namespace AvaliacaoRestaurantesAPI.Repositories
{
    public class RestauranteRepository : IRestauranteRepository
    {
        private readonly IMongoCollection<Restaurante> _restaurantes;
        private readonly IMongoCollection<Review> _review;

        public RestauranteRepository(IMongoDatabase database)
        {
            _restaurantes = database.GetCollection<Restaurante>("restaurantes");
            _review = database.GetCollection<Review>("reviews");
        }

        public async Task<List<Restaurante>> ObterTodosAsync()
        {
            return await _restaurantes.Find(_ => true).ToListAsync();
        }

        public async Task<Restaurante?> ObterPorIdAsync(string id)
        {
            return await _restaurantes.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Restaurante>> ObterPorCategoriaAsync(string categoria)
        {
            return await _restaurantes
                .Find(r => r.Categoria.ToLower() == categoria.ToLower())
                .ToListAsync();
        }

        public async Task<List<Restaurante>> ObterMelhoresAvaliadosAsync(int limite = 10)
        {
            return await _restaurantes
                .Find(_ => true)
                .SortByDescending(r => r.AvaliacaoMedia)
                .Limit(limite)
                .ToListAsync();
        }

        public async Task AtualizarMediaAvaliacaoAsync(string idRestaurante)
        {
            // Buscar todas as avaliações do restaurante
            var avaliacoes = await _review
                .Find(r => r.IdRestaurante == idRestaurante)
                .ToListAsync();

            double novaMedia = 0;
            if (avaliacoes.Any())
                novaMedia = avaliacoes.Average(r => r.Nota);

            // Atualizar a média no restaurante
            var update = Builders<Restaurante>.Update.Set(r => r.AvaliacaoMedia, novaMedia);
            await _restaurantes.UpdateOneAsync(
                r => r.Id == idRestaurante,
                update
            );
        }

    }
}
