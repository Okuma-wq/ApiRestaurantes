using API.Models;
using AvaliacaoRestaurantesAPI.DTOs;
using AvaliacaoRestaurantesAPI.Repositories;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using RestaurantesAPI.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AvaliacaoRestaurantesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioRepository _repositorio;
        private readonly IRestauranteRepository _restauranteRepositorio;
        private readonly IConfiguration _config;
        private readonly IBlobStorageService _blobStorage;

        public UsuarioController(IUsuarioRepository repositorio, IRestauranteRepository restauranteRepositorio, IConfiguration config, IBlobStorageService blobStorage)
        {
            _repositorio = repositorio;
            _restauranteRepositorio = restauranteRepositorio;
            _config = config;
            _blobStorage = blobStorage;
        }

        [HttpPost("cadastro")]
        public async Task<IActionResult> Cadastrar([FromBody] UsuarioCadastroDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuario = new Usuario
            {
                Id = Guid.NewGuid().ToString(),
                Nome = dto.Nome,
                Email = dto.Email.ToLower(),
                Senha = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
                DataCadastro = DateTime.UtcNow,
                Favoritos = new List<string?>()
            };

            await _repositorio.AdicionarAsync(usuario);
            return CreatedAtAction(nameof(ObterPorId), new { id = usuario.Id }, usuario);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UsuarioLoginDto dto)
        {
            (var token,var usuarioLogin) = await GerarToken(dto);
            if(token is null)
            {
                return Unauthorized("Credenciais inválidas.");
            }

            return Ok(new { token, usuario = new { id = usuarioLogin.Id, nome = usuarioLogin.Nome, email = usuarioLogin.Email, foto = usuarioLogin.Foto } });
        }

        [HttpPost("login/google")]
        public async Task<IActionResult> LoginGoogle([FromBody] GoogleIdTokenDto dto)
        {
            GoogleJsonWebSignature.Payload payload;

            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _config["Google:ClientId"] }
                };

                payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);
            }
            catch (InvalidJwtException)
            {
                return Unauthorized("Token do Google inválido ou expirado.");
            }

            var usuario = await _repositorio.ObterPorIdAsync(payload.Subject);

            if (usuario == null)
            {
                usuario = new Usuario
                {
                    Id = payload.Subject,
                    Nome = payload.Name,
                    Email = payload.Email.ToLower(),
                    Foto = payload.Picture,
                    DataCadastro = DateTime.UtcNow,
                    Favoritos = new List<string?>()
                };

                await _repositorio.AdicionarAsync(usuario);
            }

            var token = GerarTokenUsuario(usuario);
            return Ok(new { token, usuario = new { id = usuario.Id, nome = usuario.Nome, email = usuario.Email, foto = usuario.Foto, favoritos = usuario.Favoritos } });
        }

        [HttpGet]
        public async Task<ActionResult<List<Usuario>>> ListarTodos()
        {
            return await _repositorio.ObterTodosAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> ObterPorId(string id)
        {
            var usuario = await _repositorio.ObterPorIdAsync(id);
            if (usuario == null)
                return NoContent();
            return usuario;
        }

        [Authorize]
        [HttpPost("favoritos/{idRestaurante}")]
        public async Task<IActionResult> AdicionarAosFavoritos(string idRestaurante)
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (idUsuario == null)
                return Unauthorized("Token inválido.");

            var usuario = await _repositorio.ObterPorIdAsync(idUsuario);
            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            if (usuario.Favoritos.Contains(idRestaurante))
                return Conflict("Restaurante já está nos favoritos.");

            await _repositorio.AdicionarAosFavoritosAsync(idUsuario, idRestaurante);
            return Ok("Adicionado aos favoritos.");
        }

        [Authorize]
        [HttpDelete("favoritos/{idRestaurante}")]
        public async Task<IActionResult> RemoverDosFavoritos(string idRestaurante)
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (idUsuario == null)
                return Unauthorized("Token inválido.");

            var usuario = await _repositorio.ObterPorIdAsync(idUsuario);
            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            if (!usuario.Favoritos.Contains(idRestaurante))
                return NotFound("Restaurante não está nos favoritos.");

            await _repositorio.RemoverDosFavoritosAsync(idUsuario, idRestaurante);
            return Ok("Removido dos favoritos.");
        }

        [HttpPut("foto")]
        public async Task<IActionResult> AtualizarFotoPerfil(IFormFile foto)
        {
            if (foto == null || foto.Length == 0)
                return BadRequest("Nenhuma imagem enviada.");

            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var extensoesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extensao = Path.GetExtension(foto.FileName).ToLowerInvariant();
            if (!extensoesPermitidas.Contains(extensao))
                return BadRequest("Formato de imagem inválido. Use jpg, jpeg, png ou webp.");

            const long tamanhoMaximo = 5 * 1024 * 1024; // 5MB
            if (foto.Length > tamanhoMaximo)
                return BadRequest("A imagem deve ter no máximo 5MB.");

            var usuario = await _repositorio.ObterPorIdAsync(id);
            if (usuario == null)
                return NoContent();

            // Deletar foto antiga do blob se existir
            if (!string.IsNullOrWhiteSpace(usuario.Foto))
                await _blobStorage.DeletarFotoPerfilAsync(usuario.Foto);

            var urlFoto = await _blobStorage.UploadFotoPerfilAsync(id, foto);
            await _repositorio.AtualizarFotoAsync(id, urlFoto);

            return Ok(new { FotoUrl = urlFoto });
        }


        private async Task<(string, Usuario)> GerarToken(UsuarioLoginDto dto)
        {
            var usuario = await _repositorio.ObterPorEmailAsync(dto.Email.ToLower());
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.Senha))
                return (null, null);

            return (GerarTokenUsuario(usuario), usuario);
        }

        private string GerarTokenUsuario(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var chave = Encoding.ASCII.GetBytes(_config["Jwt:Key"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id!.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Nome),
                    new Claim(ClaimTypes.Email, usuario.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(chave), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
