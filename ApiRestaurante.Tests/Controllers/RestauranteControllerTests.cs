using AvaliacaoRestaurantesAPI.Controllers;
using AvaliacaoRestaurantesAPI.Models;
using AvaliacaoRestaurantesAPI.Repositories;
using Moq;

namespace ApiRestaurante.Tests.Controllers;

public class RestauranteControllerTests
{
    private readonly Mock<IRestauranteRepository> _repositorioMock;
    private readonly RestauranteController _controller;

    public RestauranteControllerTests()
    {
        _repositorioMock = new Mock<IRestauranteRepository>();
        _controller = new RestauranteController(_repositorioMock.Object);
    }

    [Fact]
    public async Task ListarTodos_DeveRetornarListaDeRestaurantes()
    {
        var restaurantes = new List<Restaurante>
        {
            new() { Id = "1", Nome = "Restaurante A", Categoria = "Italiana" },
            new() { Id = "2", Nome = "Restaurante B", Categoria = "Japonesa" }
        };

        _repositorioMock.Setup(r => r.ObterTodosAsync()).ReturnsAsync(restaurantes);

        var resultado = await _controller.ListarTodos();

        var valor = Assert.IsType<List<Restaurante>>(resultado.Value);
        Assert.Equal(2, valor.Count);
    }

    [Fact]
    public async Task ListarPorCategoria_DeveRetornarRestaurantesDaCategoria()
    {
        var restaurantes = new List<Restaurante>
        {
            new() { Id = "1", Nome = "Restaurante A", Categoria = "Italiana" }
        };

        _repositorioMock.Setup(r => r.ObterPorCategoriaAsync("Italiana")).ReturnsAsync(restaurantes);

        var resultado = await _controller.ListarPorCategoria("Italiana");

        var valor = Assert.IsType<List<Restaurante>>(resultado.Value);
        Assert.Single(valor);
        Assert.Equal("Italiana", valor[0].Categoria);
    }

    [Fact]
    public async Task ListarMelhoresAvaliados_DeveRetornarLista()
    {
        var restaurantes = new List<Restaurante>
        {
            new() { Id = "1", Nome = "Restaurante A", AvaliacaoMedia = 4.8 }
        };

        _repositorioMock.Setup(r => r.ObterMelhoresAvaliadosAsync(It.IsAny<int>())).ReturnsAsync(restaurantes);

        var resultado = await _controller.ListarMelhoresAvaliados();

        var valor = Assert.IsType<List<Restaurante>>(resultado.Value);
        Assert.Single(valor);
        Assert.Equal(4.8, valor[0].AvaliacaoMedia);
    }

    [Fact]
    public async Task ObterPorId_QuandoExistir_DeveRetornarRestaurante()
    {
        var restaurante = new Restaurante { Id = "1", Nome = "Restaurante A" };

        _repositorioMock.Setup(r => r.ObterPorIdAsync("1")).ReturnsAsync(restaurante);

        var resultado = await _controller.ObterPorId("1");

        var valor = Assert.IsType<Restaurante>(resultado.Value);
        Assert.Equal("1", valor.Id);
    }

    [Fact]
    public async Task ObterPorId_QuandoNaoExistir_DeveRetornarNoContent()
    {
        _repositorioMock.Setup(r => r.ObterPorIdAsync("99")).ReturnsAsync((Restaurante?)null);

        var resultado = await _controller.ObterPorId("99");

        Assert.IsType<Microsoft.AspNetCore.Mvc.NoContentResult>(resultado.Result);
    }
}
