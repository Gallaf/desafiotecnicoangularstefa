using System.ComponentModel.DataAnnotations;
using Moq;
using Pedidos.Application.DTOs.Requests;
using Pedidos.Application.Interfaces.Persistence;
using Pedidos.Application.Services;
using Pedidos.Domain.Entities;
using Pedidos.UnitTests.Builders;

namespace Pedidos.UnitTests.Application.Services;

public sealed class PedidoServiceTests
{
    private readonly Mock<IPedidoRepository> _pedidoRepository = new();
    private readonly Mock<IProdutoRepository> _produtoRepository = new();

    [Fact]
    public async Task ObterPorIdAsync_PedidoExistente_RetornaPrecoHistoricoEValorTotalCorreto()
    {
        const decimal valorAtualProduto = 250.00m;
        const decimal valorHistorico = 100.00m;
        const int quantidade = 3;

        var produto = DomainEntityBuilder.CriarProduto(1, "Produto Teste", valorAtualProduto);
        var pedido = DomainEntityBuilder.CriarPedidoCarregado(
            pedidoId: 10,
            nomeCliente: "Cliente Teste",
            emailCliente: "cliente@email.com",
            pago: true,
            produto,
            itemId: 20,
            quantidade,
            valorUnitario: valorHistorico);

        _pedidoRepository
            .Setup(repository => repository.ObterPorIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedido);

        var service = CriarService();

        var response = await service.ObterPorIdAsync(10, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(10, response.Id);
        Assert.Equal("Cliente Teste", response.NomeCliente);
        Assert.Equal("cliente@email.com", response.EmailCliente);
        Assert.True(response.Pago);
        Assert.Equal(valorHistorico * quantidade, response.ValorTotal);

        var item = Assert.Single(response.ItensPedido);
        Assert.Equal(20, item.Id);
        Assert.Equal(produto.Id, item.IdProduto);
        Assert.Equal(produto.NomeProduto, item.NomeProduto);
        Assert.Equal(valorHistorico, item.ValorUnitario);
        Assert.NotEqual(valorAtualProduto, item.ValorUnitario);
        Assert.Equal(quantidade, item.Quantidade);
    }

    [Fact]
    public async Task ObterPorIdAsync_PedidoInexistente_RetornaNull()
    {
        _pedidoRepository
            .Setup(repository => repository.ObterPorIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pedido?)null);

        var service = CriarService();

        var response = await service.ObterPorIdAsync(999, CancellationToken.None);

        Assert.Null(response);
    }

    [Fact]
    public async Task CriarAsync_DadosValidos_PersisteUmaVezComPrecoDoProduto()
    {
        const decimal valorProduto = 89.90m;
        var produto = DomainEntityBuilder.CriarProduto(1, "Mouse", valorProduto);
        var request = CriarRequest(new CriarItemPedidoRequest
        {
            ProdutoId = produto.Id,
            Quantidade = 2
        });
        Pedido? pedidoPersistido = null;

        _produtoRepository
            .Setup(repository => repository.ObterPorIdsAsync(
                It.IsAny<IReadOnlyCollection<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([produto]);

        _pedidoRepository
            .Setup(repository => repository.AdicionarAsync(
                It.IsAny<Pedido>(),
                It.IsAny<CancellationToken>()))
            .Callback<Pedido, CancellationToken>((pedido, _) =>
            {
                pedidoPersistido = pedido;
                DomainEntityBuilder.MarcarComoPersistido(pedido, 15, produto, 30);
            })
            .Returns(Task.CompletedTask);

        _pedidoRepository
            .Setup(repository => repository.ObterPorIdAsync(15, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => pedidoPersistido);

        var service = CriarService();

        var response = await service.CriarAsync(request, CancellationToken.None);

        _pedidoRepository.Verify(
            repository => repository.AdicionarAsync(
                It.IsAny<Pedido>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.NotNull(pedidoPersistido);
        var itemPersistido = Assert.Single(pedidoPersistido.ItensPedido);
        Assert.Equal(valorProduto, itemPersistido.ValorUnitario);
        Assert.Equal(15, response.Id);
        Assert.Equal(valorProduto * 2, response.ValorTotal);
        Assert.Equal(valorProduto, Assert.Single(response.ItensPedido).ValorUnitario);
    }

    [Fact]
    public async Task CriarAsync_ProdutoInexistente_LancaValidacaoENaoPersistePedido()
    {
        var request = CriarRequest(new CriarItemPedidoRequest
        {
            ProdutoId = 99,
            Quantidade = 1
        });

        _produtoRepository
            .Setup(repository => repository.ObterPorIdsAsync(
                It.IsAny<IReadOnlyCollection<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var service = CriarService();

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.CriarAsync(request, CancellationToken.None));

        Assert.Contains("99", exception.Message);
        _pedidoRepository.Verify(
            repository => repository.AdicionarAsync(
                It.IsAny<Pedido>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CriarAsync_ProdutoDuplicado_LancaValidacaoENaoPersistePedido()
    {
        var request = CriarRequest(
            new CriarItemPedidoRequest { ProdutoId = 1, Quantidade = 1 },
            new CriarItemPedidoRequest { ProdutoId = 1, Quantidade = 2 });
        var service = CriarService();

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.CriarAsync(request, CancellationToken.None));

        Assert.Contains("mais de uma vez", exception.Message);
        _pedidoRepository.Verify(
            repository => repository.AdicionarAsync(
                It.IsAny<Pedido>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _produtoRepository.Verify(
            repository => repository.ObterPorIdsAsync(
                It.IsAny<IReadOnlyCollection<int>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AtualizarAsync_PedidoExistente_AtualizaDadosEQuantidadeSemAlterarDataCriacao()
    {
        var produto = DomainEntityBuilder.CriarProduto(1, "Mouse", 150.00m);
        var pedido = DomainEntityBuilder.CriarPedidoCarregado(
            10,
            "Cliente Antigo",
            "antigo@email.com",
            false,
            produto,
            20,
            1,
            90.00m);
        var dataCriacaoOriginal = pedido.DataCriacao;
        var request = new CriarPedidoRequest
        {
            NomeCliente = "Cliente Atualizado",
            EmailCliente = "atualizado@email.com",
            Pago = true,
            Itens = [new CriarItemPedidoRequest { ProdutoId = produto.Id, Quantidade = 4 }]
        };
        ConfigurarAtualizacao(pedido, produto);

        var response = await CriarService().AtualizarAsync(10, request, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal("Cliente Atualizado", response.NomeCliente);
        Assert.Equal("atualizado@email.com", response.EmailCliente);
        Assert.True(response.Pago);
        Assert.Equal(4, Assert.Single(response.ItensPedido).Quantidade);
        Assert.Equal(dataCriacaoOriginal, pedido.DataCriacao);
        _pedidoRepository.Verify(
            repository => repository.AtualizarAsync(pedido, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_ProdutoJaExistente_PreservaValorUnitarioHistorico()
    {
        const decimal valorHistorico = 75.00m;
        var produto = DomainEntityBuilder.CriarProduto(1, "Mouse", 200.00m);
        var pedido = DomainEntityBuilder.CriarPedidoCarregado(
            10,
            "Cliente Teste",
            "cliente@email.com",
            false,
            produto,
            20,
            1,
            valorHistorico);
        var request = CriarRequest(
            new CriarItemPedidoRequest { ProdutoId = produto.Id, Quantidade = 3 });
        ConfigurarAtualizacao(pedido, produto);

        var response = await CriarService().AtualizarAsync(10, request, CancellationToken.None);

        Assert.NotNull(response);
        var item = Assert.Single(response.ItensPedido);
        Assert.Equal(valorHistorico, item.ValorUnitario);
        Assert.Equal(valorHistorico * 3, response.ValorTotal);
    }

    [Fact]
    public async Task AtualizarAsync_ProdutoNovo_UsaPrecoAtualDoProduto()
    {
        const decimal valorProdutoNovo = 350.00m;
        var produtoExistente = DomainEntityBuilder.CriarProduto(1, "Mouse", 200.00m);
        var produtoNovo = DomainEntityBuilder.CriarProduto(2, "Teclado", valorProdutoNovo);
        var pedido = DomainEntityBuilder.CriarPedidoCarregado(
            10,
            "Cliente Teste",
            "cliente@email.com",
            false,
            produtoExistente,
            20,
            1,
            75.00m);
        var request = CriarRequest(
            new CriarItemPedidoRequest { ProdutoId = produtoExistente.Id, Quantidade = 1 },
            new CriarItemPedidoRequest { ProdutoId = produtoNovo.Id, Quantidade = 2 });

        _pedidoRepository
            .Setup(repository => repository.ObterParaAtualizacaoAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedido);
        _produtoRepository
            .Setup(repository => repository.ObterPorIdsAsync(
                It.IsAny<IReadOnlyCollection<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([produtoExistente, produtoNovo]);
        _pedidoRepository
            .Setup(repository => repository.AtualizarAsync(pedido, It.IsAny<CancellationToken>()))
            .Callback<Pedido, CancellationToken>((pedidoAtualizado, _) =>
            {
                var itemNovo = pedidoAtualizado.ItensPedido.Single(item => item.ProdutoId == produtoNovo.Id);
                DomainEntityBuilder.MarcarItemComoPersistido(pedidoAtualizado, itemNovo, produtoNovo, 30);
            })
            .Returns(Task.CompletedTask);
        _pedidoRepository
            .Setup(repository => repository.ObterPorIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedido);

        var response = await CriarService().AtualizarAsync(10, request, CancellationToken.None);

        Assert.NotNull(response);
        var itemNovoResponse = Assert.Single(
            response.ItensPedido,
            item => item.IdProduto == produtoNovo.Id);
        Assert.Equal(valorProdutoNovo, itemNovoResponse.ValorUnitario);
        Assert.Equal(2, itemNovoResponse.Quantidade);
    }

    [Fact]
    public async Task AtualizarAsync_EntradaInvalida_NaoPersiste()
    {
        var produto = DomainEntityBuilder.CriarProduto(1, "Mouse", 100.00m);
        var pedido = DomainEntityBuilder.CriarPedidoCarregado(
            10,
            "Cliente Teste",
            "cliente@email.com",
            false,
            produto,
            20,
            1,
            75.00m);
        var request = CriarRequest(
            new CriarItemPedidoRequest { ProdutoId = produto.Id, Quantidade = 0 });
        _pedidoRepository
            .Setup(repository => repository.ObterParaAtualizacaoAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedido);

        await Assert.ThrowsAsync<ValidationException>(
            () => CriarService().AtualizarAsync(10, request, CancellationToken.None));

        _pedidoRepository.Verify(
            repository => repository.AtualizarAsync(
                It.IsAny<Pedido>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _produtoRepository.Verify(
            repository => repository.ObterPorIdsAsync(
                It.IsAny<IReadOnlyCollection<int>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RemoverAsync_PedidoExistente_RemoveNoRepository()
    {
        _pedidoRepository
            .Setup(repository => repository.RemoverAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var removido = await CriarService().RemoverAsync(10, CancellationToken.None);

        Assert.True(removido);
        _pedidoRepository.Verify(
            repository => repository.RemoverAsync(10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private PedidoService CriarService()
    {
        return new PedidoService(_pedidoRepository.Object, _produtoRepository.Object);
    }

    private void ConfigurarAtualizacao(Pedido pedido, params Produto[] produtos)
    {
        _pedidoRepository
            .Setup(repository => repository.ObterParaAtualizacaoAsync(
                pedido.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedido);
        _produtoRepository
            .Setup(repository => repository.ObterPorIdsAsync(
                It.IsAny<IReadOnlyCollection<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(produtos);
        _pedidoRepository
            .Setup(repository => repository.AtualizarAsync(
                pedido,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _pedidoRepository
            .Setup(repository => repository.ObterPorIdAsync(
                pedido.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedido);
    }

    private static CriarPedidoRequest CriarRequest(params CriarItemPedidoRequest[] itens)
    {
        return new CriarPedidoRequest
        {
            NomeCliente = "Cliente Teste",
            EmailCliente = "cliente@email.com",
            Pago = false,
            Itens = itens
        };
    }
}
