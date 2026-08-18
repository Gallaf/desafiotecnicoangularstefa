using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pedidos.Api.Controllers;
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
}
