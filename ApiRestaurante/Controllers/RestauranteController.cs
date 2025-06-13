using AvaliacaoRestaurantesAPI.DTOs;
using AvaliacaoRestaurantesAPI.Models;
using AvaliacaoRestaurantesAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AvaliacaoRestaurantesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RestauranteController : ControllerBase
    {
        private readonly IRestauranteRepository _repositorio;

        public RestauranteController(IRestauranteRepository repositorio)
        {
            _repositorio = repositorio;
        }

        [HttpGet]
        public async Task<ActionResult<List<Restaurante>>> ListarTodos()
        {
            return await _repositorio.ObterTodosAsync();
        }

        [HttpGet("categoria/{categoria}")]
        public async Task<ActionResult<List<Restaurante>>> ListarPorCategoria(string categoria)
        {
            return await _repositorio.ObterPorCategoriaAsync(categoria);
        }

        [HttpGet("melhores")]
        public async Task<ActionResult<List<Restaurante>>> ListarMelhoresAvaliados()
        {
            return await _repositorio.ObterMelhoresAvaliadosAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Restaurante>> ObterPorId(string id)
        {
            var restaurante = await _repositorio.ObterPorIdAsync(id);
            if (restaurante == null)
                return NotFound();
            return restaurante;
        }
    }
}
