using AvaliacaoRestaurantesAPI.DTOs;
using AvaliacaoRestaurantesAPI.Models;
using AvaliacaoRestaurantesAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantesAPI.Interfaces;
using System.Security.Claims;

namespace AvaliacaoRestaurantesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewRepository _reviewRepositorio;
        private readonly IRestauranteRepository _restauranteRepositorio;

        public ReviewController(IReviewRepository reviewRepositorio, IRestauranteRepository restauranteRepositorio)
        {
            _reviewRepositorio = reviewRepositorio;
            _restauranteRepositorio = restauranteRepositorio;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Criar([FromBody] ReviewCriarDto dto)
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (idUsuario == null)
            {
                return Unauthorized("Usuário inválido.");
            }

            var review = new Review
            {
                Id = Guid.NewGuid().ToString(),
                IdRestaurante = dto.IdRestaurante,
                IdUsuario = idUsuario,
                Nota = dto.Nota,
                Comentario = dto.Comentario,
                Data = DateTime.UtcNow
            };

            var restaurante = await _restauranteRepositorio.ObterPorIdAsync(dto.IdRestaurante!);
            if (restaurante == null)
            {
                return BadRequest("Restaurante não encontrado.");
            }

            await _reviewRepositorio.AdicionarAsync(review);
            await _restauranteRepositorio.AtualizarMediaAvaliacaoAsync(dto.IdRestaurante!);

            return CreatedAtAction(nameof(ObterPorId), new { id = review.Id }, review);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Review>> ObterPorId(string id)
        {
            var review = await _reviewRepositorio.ObterPorIdAsync(id);
            if (review == null)
                return NotFound();
            return review;
        }

        [HttpGet("restaurante/{idRestaurante}")]
        public async Task<ActionResult<List<Review>>> ListarPorRestaurante(string idRestaurante)
        {
            return await _reviewRepositorio.ObterPorRestauranteAsync(idRestaurante);
        }

        [HttpGet("usuario")]
        public async Task<ActionResult<List<Review>>> ListarPorUsuario()
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (idUsuario == null )
                return Unauthorized("Token inválido ou sem identificador de usuário.");

            return await _reviewRepositorio.ObterPorUsuarioAsync(idUsuario);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(string id, [FromBody] ReviewAlterarDto dto)
        {
            var existente = await _reviewRepositorio.ObterPorIdAsync(id);
            if (existente == null)
                return NotFound();

            existente.Nota = dto.Nota;
            existente.Comentario = dto.Comentario;

            await _reviewRepositorio.AtualizarAsync(existente);
            await _restauranteRepositorio.AtualizarMediaAvaliacaoAsync(existente.IdRestaurante!);

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(string id)
        {
            var review = await _reviewRepositorio.ObterPorIdAsync(id);
            if (review == null)
                return NotFound();

            await _reviewRepositorio.RemoverAsync(id);
            await _restauranteRepositorio.AtualizarMediaAvaliacaoAsync(review.IdRestaurante!);

            return NoContent();
        }
    }
}
