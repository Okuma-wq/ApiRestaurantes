using API.Models;
using AvaliacaoRestaurantesAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
        private readonly IConfiguration _config;
        private readonly IBlobStorageService _blobStorage;

        public UsuarioController(IUsuarioRepository repositorio, IConfiguration config, IBlobStorageService blobStorage)
        {
            _repositorio = repositorio;
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
            var token = await GerarToken(dto);
            if(token is null)
            {
                return Unauthorized("Credenciais inválidas.");
            }

            return Ok(new { token });
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

        [HttpPost("{idUsuario}/favoritos/{idRestaurante}")]
        public async Task<IActionResult> AdicionarAosFavoritos(string idUsuario, string idRestaurante)
        {
            var usuario = await _repositorio.ObterPorIdAsync(idUsuario);
            if (usuario == null)
                return NoContent();

            await _repositorio.AdicionarAosFavoritosAsync(idUsuario, idRestaurante);
            return Ok("Adicionado aos favoritos.");
        }

        [HttpPut("{id}/foto")]
        public async Task<IActionResult> AtualizarFotoPerfil(string id, IFormFile foto)
        {
            if (foto == null || foto.Length == 0)
                return BadRequest("Nenhuma imagem enviada.");

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


        private async Task<string> GerarToken(UsuarioLoginDto dto)
        {
            var usuario = await _repositorio.ObterPorEmailAsync(dto.Email.ToLower());
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.Senha))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var chave = Encoding.ASCII.GetBytes(_config["Jwt:Key"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome)
            }),
                Expires = DateTime.UtcNow.AddHours(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(chave), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
