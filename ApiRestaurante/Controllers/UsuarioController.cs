using API.Models;
using AvaliacaoRestaurantesAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RestaurantesAPI.Interfaces;
using System;
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

        public UsuarioController(IUsuarioRepository repositorio, IConfiguration config)
        {
            _repositorio = repositorio;
            _config = config;
        }

        [HttpPost("cadastro")]
        public async Task<IActionResult> Cadastrar([FromBody] UsuarioCadastroDto dto)
        {
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
                return NotFound();
            return usuario;
        }

        [HttpPost("{idUsuario}/favoritos/{idRestaurante}")]
        public async Task<IActionResult> AdicionarAosFavoritos(string idUsuario, string idRestaurante)
        {
            var usuario = await _repositorio.ObterPorIdAsync(idUsuario);
            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            await _repositorio.AdicionarAosFavoritosAsync(idUsuario, idRestaurante);
            return Ok("Adicionado aos favoritos.");
        }


        private async Task<string> GerarToken(UsuarioLoginDto dto)
        {
            var usuario = await _repositorio.ObterPorEmailAsync(dto.Email);
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
