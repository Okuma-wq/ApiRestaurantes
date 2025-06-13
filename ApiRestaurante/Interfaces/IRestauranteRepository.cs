using AvaliacaoRestaurantesAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaliacaoRestaurantesAPI.Repositories
{
    public interface IRestauranteRepository
    {
        Task<List<Restaurante>> ObterTodosAsync();
        Task<Restaurante?> ObterPorIdAsync(string id);
        Task<List<Restaurante>> ObterPorCategoriaAsync(string categoria);
        Task<List<Restaurante>> ObterMelhoresAvaliadosAsync(int limite = 10);
        Task AtualizarMediaAvaliacaoAsync(string idRestaurante);
    }
}
