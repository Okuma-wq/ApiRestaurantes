using AvaliacaoRestaurantesAPI.Models;
using MongoDB.Driver;
using RestaurantesAPI.Interfaces;

namespace AvaliacaoRestaurantesAPI.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly IMongoCollection<Review> _reviews;

        public ReviewRepository(IMongoDatabase database)
        {
            _reviews = database.GetCollection<Review>("reviews");
        }

        public async Task<List<Review>> ObterPorRestauranteAsync(string idRestaurante)
        {
            return await _reviews.Find(r => r.IdRestaurante == idRestaurante).ToListAsync();
        }

        public async Task<List<Review>> ObterPorUsuarioAsync(string idUsuario)
        {
            return await _reviews.Find(r => r.IdUsuario == idUsuario).ToListAsync();
        }

        public async Task<Review?> ObterPorIdAsync(string id)
        {
            return await _reviews.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        public async Task AdicionarAsync(Review review)
        {
            await _reviews.InsertOneAsync(review);
        }

        public async Task AtualizarAsync(Review review)
        {
            await _reviews.ReplaceOneAsync(r => r.Id == review.Id, review);
        }

        public async Task RemoverAsync(string id)
        {
            await _reviews.DeleteOneAsync(r => r.Id == id);
        }
    }
}
