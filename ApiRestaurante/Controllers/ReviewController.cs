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
        private readonly IBlobStorageService _blobStorage;

        private static readonly string[] _extensoesPermitidas = [".jpg", ".jpeg", ".png", ".webp"];
        private const long TamanhoMaximoPorArquivo = 5 * 1024 * 1024; // 5MB

        public ReviewController(IReviewRepository reviewRepositorio, IRestauranteRepository restauranteRepositorio, IBlobStorageService blobStorage)
        {
            _reviewRepositorio = reviewRepositorio;
            _restauranteRepositorio = restauranteRepositorio;
            _blobStorage = blobStorage;
        }

        [HttpPost]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Criar([FromForm] ReviewCriarDto dto)
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (idUsuario == null)
                return Unauthorized("Usuário inválido.");

            if (dto.Fotos != null)
            {
                var erroFotos = ValidarFotos(dto.Fotos);
                if (erroFotos != null)
                    return BadRequest(erroFotos);
            }

            var restaurante = await _restauranteRepositorio.ObterPorIdAsync(dto.IdRestaurante!);
            if (restaurante == null)
            {
                if (string.IsNullOrWhiteSpace(dto.NomeRestaurante))
                    return BadRequest("Restaurante não encontrado. Informe o NomeRestaurante para criá-lo automaticamente.");

                restaurante = new Restaurante
                {
                    Id = dto.IdRestaurante,
                    Nome = dto.NomeRestaurante
                };
                await _restauranteRepositorio.AdicionarAsync(restaurante);
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

            if (dto.Fotos != null && dto.Fotos.Count > 0)
                review.Fotos = (await _blobStorage.UploadFotosReviewAsync(review.Id, dto.Fotos))!;

            await _reviewRepositorio.AdicionarAsync(review);
            await _restauranteRepositorio.AtualizarMediaAvaliacaoAsync(dto.IdRestaurante!);

            return CreatedAtAction(nameof(ObterPorId), new { id = review.Id }, review);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Review>> ObterPorId(string id)
        {
            var review = await _reviewRepositorio.ObterPorIdAsync(id);
            if (review == null)
                return NoContent();
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

            if (idUsuario == null)
                return Unauthorized("Token inválido ou sem identificador de usuário.");

            return await _reviewRepositorio.ObterPorUsuarioAsync(idUsuario);
        }

        [Authorize]
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Atualizar(string id, [FromForm] ReviewAlterarDto dto)
        {
            var existente = await _reviewRepositorio.ObterPorIdAsync(id);
            if (existente == null)
                return NoContent();

            if (dto.Fotos != null)
            {
                var erroFotos = ValidarFotos(dto.Fotos);
                if (erroFotos != null)
                    return BadRequest(erroFotos);
            }

            existente.Nota = dto.Nota;
            existente.Comentario = dto.Comentario;

            // Substitui fotos se enviadas, senão mantém as existentes
            if (dto.Fotos != null && dto.Fotos.Count > 0)
            {
                if (existente.Fotos.Any())
                    await _blobStorage.DeletarFotosReviewAsync(existente.Fotos);

                existente.Fotos = (await _blobStorage.UploadFotosReviewAsync(id, dto.Fotos))!;
            }

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
                return NoContent();

            if (review.Fotos.Any())
                await _blobStorage.DeletarFotosReviewAsync(review.Fotos);

            await _reviewRepositorio.RemoverAsync(id);
            await _restauranteRepositorio.AtualizarMediaAvaliacaoAsync(review.IdRestaurante!);

            return NoContent();
        }

        private static string? ValidarFotos(IList<IFormFile> fotos)
        {
            if (fotos.Count > 5)
                return "Máximo de 5 imagens por review.";

            foreach (var foto in fotos)
            {
                var extensao = Path.GetExtension(foto.FileName).ToLowerInvariant();
                if (!_extensoesPermitidas.Contains(extensao))
                    return $"Arquivo '{foto.FileName}': formato inválido. Use jpg, jpeg, png ou webp.";

                if (foto.Length > TamanhoMaximoPorArquivo)
                    return $"Arquivo '{foto.FileName}': tamanho máximo é 5MB.";
            }

            return null;
        }
    }
}
