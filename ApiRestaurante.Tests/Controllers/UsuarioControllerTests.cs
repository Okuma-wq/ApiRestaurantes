using API.Models;
using AvaliacaoRestaurantesAPI.Controllers;
using AvaliacaoRestaurantesAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using RestaurantesAPI.Interfaces;

namespace ApiRestaurante.Tests.Controllers;

public class UsuarioControllerTests
{
    private readonly Mock<IUsuarioRepository> _repositorioMock;
    private readonly IConfiguration _config;
    private readonly UsuarioController _controller;

    public UsuarioControllerTests()
    {
        _repositorioMock = new Mock<IUsuarioRepository>();

        var configValues = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "1YGuuvB6YFg9sW6ZKMCpPx7XZ+z7Y5dIGI0MHhv6lbI=",
            ["Jwt:Issuer"] = "RestaurantesAPI",
            ["Jwt:Audience"] = "RestaurantesAPI"
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        _controller = new UsuarioController(_repositorioMock.Object, _config);
    }

    [Fact]
    public async Task Cadastrar_ComModelStateInvalido_DeveRetornarBadRequest()
    {
        _controller.ModelState.AddModelError("Email", "O e-mail informado é inválido.");

        var dto = new UsuarioCadastroDto
        {
            Nome = "Pe",
            Email = "email-invalido",
            Senha = "123"
        };

        var resultado = await _controller.Cadastrar(dto);

        Assert.IsType<BadRequestObjectResult>(resultado);
        _repositorioMock.Verify(r => r.AdicionarAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task Cadastrar_DeveRetornarCreatedAtAction()
    {
        var dto = new UsuarioCadastroDto
        {
            Nome = "Pedro",
            Email = "PEDRO@EMAIL.COM",
            Senha = "123456"
        };

        var resultado = await _controller.Cadastrar(dto);

        var created = Assert.IsType<CreatedAtActionResult>(resultado);
        Assert.Equal(nameof(UsuarioController.ObterPorId), created.ActionName);

        var usuario = Assert.IsType<Usuario>(created.Value);
        Assert.Equal("pedro@email.com", usuario.Email);
        Assert.NotEqual(dto.Senha, usuario.Senha);

        _repositorioMock.Verify(r => r.AdicionarAsync(It.IsAny<Usuario>()), Times.Once);
    }

    [Fact]
    public async Task Login_ComCredenciaisInvalidas_DeveRetornarUnauthorized()
    {
        var dto = new UsuarioLoginDto
        {
            Email = "invalido@email.com",
            Senha = "123456"
        };

        _repositorioMock.Setup(r => r.ObterPorEmailAsync(dto.Email.ToLower()))
            .ReturnsAsync((Usuario?)null);

        var resultado = await _controller.Login(dto);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(resultado);
        Assert.Equal("Credenciais inválidas.", unauthorized.Value);
    }

    [Fact]
    public async Task Login_ComSenhaInvalida_DeveRetornarUnauthorized()
    {
        var dto = new UsuarioLoginDto
        {
            Email = "usuario@email.com",
            Senha = "senha-errada"
        };

        var usuario = new Usuario
        {
            Id = "1",
            Nome = "Usuário",
            Email = dto.Email,
            Senha = BCrypt.Net.BCrypt.HashPassword("123456")
        };

        _repositorioMock.Setup(r => r.ObterPorEmailAsync(dto.Email.ToLower()))
            .ReturnsAsync(usuario);

        var resultado = await _controller.Login(dto);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(resultado);
        Assert.Equal("Credenciais inválidas.", unauthorized.Value);
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarOkComToken()
    {
        var senha = "123456";
        var dto = new UsuarioLoginDto
        {
            Email = "usuario@email.com",
            Senha = senha
        };

        var usuario = new Usuario
        {
            Id = "1",
            Nome = "Usuário",
            Email = dto.Email,
            Senha = BCrypt.Net.BCrypt.HashPassword(senha)
        };

        _repositorioMock.Setup(r => r.ObterPorEmailAsync(dto.Email.ToLower()))
            .ReturnsAsync(usuario);

        var resultado = await _controller.Login(dto);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        var valor = ok.Value;
        Assert.NotNull(valor);
        var tokenProperty = valor!.GetType().GetProperty("token");
        Assert.NotNull(tokenProperty);
        var token = tokenProperty!.GetValue(valor) as string;
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task ListarTodos_DeveRetornarListaDeUsuarios()
    {
        var usuarios = new List<Usuario>
        {
            new() { Id = "1", Nome = "Pedro", Email = "pedro@email.com" }
        };

        _repositorioMock.Setup(r => r.ObterTodosAsync()).ReturnsAsync(usuarios);

        var resultado = await _controller.ListarTodos();

        var valor = Assert.IsType<List<Usuario>>(resultado.Value);
        Assert.Single(valor);
    }

    [Fact]
    public async Task ObterPorId_QuandoExistir_DeveRetornarUsuario()
    {
        var usuario = new Usuario { Id = "1", Nome = "Pedro", Email = "pedro@email.com" };
        _repositorioMock.Setup(r => r.ObterPorIdAsync("1")).ReturnsAsync(usuario);

        var resultado = await _controller.ObterPorId("1");

        var valor = Assert.IsType<Usuario>(resultado.Value);
        Assert.Equal("1", valor.Id);
    }

    [Fact]
    public async Task ObterPorId_QuandoNaoExistir_DeveRetornarNoContent()
    {
        _repositorioMock.Setup(r => r.ObterPorIdAsync("99")).ReturnsAsync((Usuario?)null);

        var resultado = await _controller.ObterPorId("99");

        Assert.IsType<NoContentResult>(resultado.Result);
    }

    [Fact]
    public async Task AdicionarAosFavoritos_QuandoUsuarioNaoExistir_DeveRetornarNoContent()
    {
        _repositorioMock.Setup(r => r.ObterPorIdAsync("1")).ReturnsAsync((Usuario?)null);

        var resultado = await _controller.AdicionarAosFavoritos("1", "10");

        Assert.IsType<NoContentResult>(resultado);
        _repositorioMock.Verify(r => r.AdicionarAosFavoritosAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AdicionarAosFavoritos_QuandoUsuarioExistir_DeveRetornarOk()
    {
        _repositorioMock.Setup(r => r.ObterPorIdAsync("1"))
            .ReturnsAsync(new Usuario { Id = "1", Nome = "Pedro", Email = "pedro@email.com" });

        var resultado = await _controller.AdicionarAosFavoritos("1", "10");

        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal("Adicionado aos favoritos.", ok.Value);
        _repositorioMock.Verify(r => r.AdicionarAosFavoritosAsync("1", "10"), Times.Once);
    }
}
