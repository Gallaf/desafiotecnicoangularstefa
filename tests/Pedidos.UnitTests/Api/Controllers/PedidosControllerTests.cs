using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pedidos.Api.Controllers;
using Pedidos.Application.DTOs.Requests;
using Pedidos.Application.DTOs.Responses;
using Pedidos.Application.Interfaces;

namespace Pedidos.UnitTests.Api.Controllers;

public sealed class PedidosControllerTests
{
    private readonly Mock<IPedidoService> _pedidoService = new();

    [Fact]
    public async Task ObterPorId_PedidoExistente_RetornaOkComResponseDoService()
    {
        var responseEsperado = new PedidoResponse
        {
            Id = 10,
            NomeCliente = "Cliente Teste",
            EmailCliente = "cliente@email.com",
            Pago = false,
            ValorTotal = 200.00m,
            ItensPedido = []
        };
        _pedidoService
            .Setup(service => service.ObterPorIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseEsperado);
        var controller = new PedidosController(_pedidoService.Object);

        var result = await controller.ObterPorId(10, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Same(responseEsperado, okResult.Value);
    }

    [Fact]
    public async Task ObterPorId_PedidoInexistente_RetornaNotFoundComProblemDetails()
    {
        _pedidoService
            .Setup(service => service.ObterPorIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PedidoResponse?)null);
        var controller = new PedidosController(_pedidoService.Object);

        var result = await controller.ObterPorId(999, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);

        var problemDetails = Assert.IsType<ProblemDetails>(notFoundResult.Value);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
        Assert.Equal("Pedido não encontrado.", problemDetails.Title);
        Assert.Contains("999", problemDetails.Detail);
    }

    [Fact]
    public async Task Atualizar_PedidoInexistente_RetornaNotFoundComProblemDetails()
    {
        var request = CriarRequest();
        _pedidoService
            .Setup(service => service.AtualizarAsync(
                999,
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PedidoResponse?)null);
        var controller = new PedidosController(_pedidoService.Object);

        var result = await controller.Atualizar(999, request, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(notFoundResult.Value);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
        Assert.Contains("999", problemDetails.Detail);
    }

    [Fact]
    public async Task Remover_PedidoExistente_RetornaNoContent()
    {
        _pedidoService
            .Setup(service => service.RemoverAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var controller = new PedidosController(_pedidoService.Object);

        var result = await controller.Remover(10, CancellationToken.None);

        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }

    [Fact]
    public async Task Remover_PedidoInexistente_RetornaNotFoundComProblemDetails()
    {
        _pedidoService
            .Setup(service => service.RemoverAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var controller = new PedidosController(_pedidoService.Object);

        var result = await controller.Remover(999, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(notFoundResult.Value);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
        Assert.Contains("999", problemDetails.Detail);
    }

    private static CriarPedidoRequest CriarRequest()
    {
        return new CriarPedidoRequest
        {
            NomeCliente = "Cliente Teste",
            EmailCliente = "cliente@email.com",
            Pago = false,
            Itens = [new CriarItemPedidoRequest { ProdutoId = 1, Quantidade = 1 }]
        };
    }
}
