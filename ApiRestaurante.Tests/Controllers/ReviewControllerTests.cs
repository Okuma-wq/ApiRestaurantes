using System.Security.Claims;
using AvaliacaoRestaurantesAPI.Controllers;
using AvaliacaoRestaurantesAPI.DTOs;
using AvaliacaoRestaurantesAPI.Models;
using AvaliacaoRestaurantesAPI.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestaurantesAPI.Interfaces;

namespace ApiRestaurante.Tests.Controllers;

public class ReviewControllerTests
{
    private readonly Mock<IReviewRepository> _reviewRepositorioMock;
    private readonly Mock<IRestauranteRepository> _restauranteRepositorioMock;
    private readonly ReviewController _controller;

    public ReviewControllerTests()
    {
        _reviewRepositorioMock = new Mock<IReviewRepository>();
        _restauranteRepositorioMock = new Mock<IRestauranteRepository>();
        _controller = new ReviewController(_reviewRepositorioMock.Object, _restauranteRepositorioMock.Object);
    }

    [Fact]
    public async Task ObterPorId_QuandoExistir_DeveRetornarReview()
    {
        var review = new Review { Id = "1", IdRestaurante = "10", IdUsuario = "20", Nota = 5, Comentario = "Ótimo" };
        _reviewRepositorioMock.Setup(r => r.ObterPorIdAsync("1")).ReturnsAsync(review);

        var resultado = await _controller.ObterPorId("1");

        var valor = Assert.IsType<Review>(resultado.Value);
        Assert.Equal("1", valor.Id);
    }

    [Fact]
    public async Task ObterPorId_QuandoNaoExistir_DeveRetornarNoContent()
    {
        _reviewRepositorioMock.Setup(r => r.ObterPorIdAsync("99")).ReturnsAsync((Review?)null);

        var resultado = await _controller.ObterPorId("99");

        Assert.IsType<NoContentResult>(resultado.Result);
    }

    [Fact]
    public async Task ListarPorRestaurante_DeveRetornarLista()
    {
        var reviews = new List<Review>
        {
            new() { Id = "1", IdRestaurante = "10", Nota = 5, Comentario = "Ótimo" }
        };

        _reviewRepositorioMock.Setup(r => r.ObterPorRestauranteAsync("10")).ReturnsAsync(reviews);

        var resultado = await _controller.ListarPorRestaurante("10");

        var valor = Assert.IsType<List<Review>>(resultado.Value);
        Assert.Single(valor);
    }

    [Fact]
    public async Task ListarPorUsuario_SemClaim_DeveRetornarUnauthorized()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var resultado = await _controller.ListarPorUsuario();

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(resultado.Result);
        Assert.Equal("Token inválido ou sem identificador de usuário.", unauthorized.Value);
    }

    [Fact]
    public async Task ListarPorUsuario_ComClaim_DeveRetornarLista()
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "20") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var reviews = new List<Review>
        {
            new() { Id = "1", IdUsuario = "20", IdRestaurante = "10", Nota = 4, Comentario = "Bom" }
        };

        _reviewRepositorioMock.Setup(r => r.ObterPorUsuarioAsync("20")).ReturnsAsync(reviews);

        var resultado = await _controller.ListarPorUsuario();

        var valor = Assert.IsType<List<Review>>(resultado.Value);
        Assert.Single(valor);
    }

    [Fact]
    public async Task Criar_SemUsuarioAutenticado_DeveLancarExcecao()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var dto = new ReviewCriarDto { IdRestaurante = "10", Nota = 5, Comentario = "Ótimo" };

        await Assert.ThrowsAsync<NullReferenceException>(() => _controller.Criar(dto));
    }

    [Fact]
    public async Task Criar_QuandoRestauranteNaoExistir_DeveRetornarBadRequest()
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "20") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var dto = new ReviewCriarDto { IdRestaurante = "10", Nota = 5, Comentario = "Ótimo" };
        _restauranteRepositorioMock.Setup(r => r.ObterPorIdAsync("10")).ReturnsAsync((Restaurante?)null);

        var resultado = await _controller.Criar(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal("Restaurante não encontrado.", badRequest.Value);
    }

    [Fact]
    public async Task Criar_QuandoDadosValidos_DeveRetornarCreatedAtAction()
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "20") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var dto = new ReviewCriarDto { IdRestaurante = "10", Nota = 5, Comentario = "Ótimo" };
        _restauranteRepositorioMock.Setup(r => r.ObterPorIdAsync("10")).ReturnsAsync(new Restaurante { Id = "10", Nome = "Restaurante A" });

        var resultado = await _controller.Criar(dto);

        var created = Assert.IsType<CreatedAtActionResult>(resultado);
        Assert.Equal(nameof(ReviewController.ObterPorId), created.ActionName);
        _reviewRepositorioMock.Verify(r => r.AdicionarAsync(It.IsAny<Review>()), Times.Once);
        _restauranteRepositorioMock.Verify(r => r.AtualizarMediaAvaliacaoAsync("10"), Times.Once);
    }

    [Fact]
    public async Task Atualizar_QuandoReviewNaoExistir_DeveRetornarNoContent()
    {
        _reviewRepositorioMock.Setup(r => r.ObterPorIdAsync("1")).ReturnsAsync((Review?)null);

        var resultado = await _controller.Atualizar("1", new ReviewAlterarDto { Nota = 4, Comentario = "Atualizado" });

        Assert.IsType<NoContentResult>(resultado);
    }

    [Fact]
    public async Task Atualizar_QuandoReviewExistir_DeveRetornarNoContent()
    {
        var review = new Review { Id = "1", IdRestaurante = "10", Nota = 3, Comentario = "Antigo" };
        _reviewRepositorioMock.Setup(r => r.ObterPorIdAsync("1")).ReturnsAsync(review);

        var resultado = await _controller.Atualizar("1", new ReviewAlterarDto { Nota = 5, Comentario = "Novo" });

        Assert.IsType<NoContentResult>(resultado);
        _reviewRepositorioMock.Verify(r => r.AtualizarAsync(It.Is<Review>(x => x.Nota == 5 && x.Comentario == "Novo")), Times.Once);
        _restauranteRepositorioMock.Verify(r => r.AtualizarMediaAvaliacaoAsync("10"), Times.Once);
    }

    [Fact]
    public async Task Deletar_QuandoReviewNaoExistir_DeveRetornarNoContent()
    {
        _reviewRepositorioMock.Setup(r => r.ObterPorIdAsync("1")).ReturnsAsync((Review?)null);

        var resultado = await _controller.Deletar("1");

        Assert.IsType<NoContentResult>(resultado);
    }

    [Fact]
    public async Task Deletar_QuandoReviewExistir_DeveRetornarNoContent()
    {
        var review = new Review { Id = "1", IdRestaurante = "10" };
        _reviewRepositorioMock.Setup(r => r.ObterPorIdAsync("1")).ReturnsAsync(review);

        var resultado = await _controller.Deletar("1");

        Assert.IsType<NoContentResult>(resultado);
        _reviewRepositorioMock.Verify(r => r.RemoverAsync("1"), Times.Once);
        _restauranteRepositorioMock.Verify(r => r.AtualizarMediaAvaliacaoAsync("10"), Times.Once);
    }
}
