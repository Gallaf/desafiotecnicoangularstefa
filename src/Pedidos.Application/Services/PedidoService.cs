using System.ComponentModel.DataAnnotations;
using Pedidos.Application.DTOs.Requests;
using Pedidos.Application.DTOs.Responses;
using Pedidos.Application.Interfaces;
using Pedidos.Application.Interfaces.Persistence;
using Pedidos.Domain.Entities;

namespace Pedidos.Application.Services;

public sealed class PedidoService(
    IPedidoRepository pedidoRepository,
    IProdutoRepository produtoRepository) : IPedidoService
{
    public async Task<PedidoResponse> CriarAsync(
        CriarPedidoRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidarRequest(request);

        var produtosIds = request.Itens
            .Select(item => item.ProdutoId)
            .ToArray();

        var produtos = await produtoRepository.ObterPorIdsAsync(produtosIds, cancellationToken);
        var produtosPorId = produtos.ToDictionary(produto => produto.Id);
        var produtosInexistentes = produtosIds
            .Where(id => !produtosPorId.ContainsKey(id))
            .Order()
            .ToArray();

        if (produtosInexistentes.Length > 0)
        {
            throw new ValidationException(
                $"Produto(s) não encontrado(s): {string.Join(", ", produtosInexistentes)}.");
        }

        var pedido = new Pedido(
            request.NomeCliente,
            request.EmailCliente,
            DateTime.UtcNow,
            request.Pago);

        foreach (var item in request.Itens)
        {
            var produto = produtosPorId[item.ProdutoId];
            pedido.AdicionarItem(produto.Id, item.Quantidade, produto.Valor);
        }

        await pedidoRepository.AdicionarAsync(pedido, cancellationToken);

        var pedidoCriado = await pedidoRepository.ObterPorIdAsync(pedido.Id, cancellationToken)
            ?? throw new InvalidOperationException("Não foi possível carregar o pedido criado.");

        return MapearResponse(pedidoCriado);
    }

    public async Task<PedidoResponse?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return null;
        }

        var pedido = await pedidoRepository.ObterPorIdAsync(id, cancellationToken);

        return pedido is null ? null : MapearResponse(pedido);
    }

    private static void ValidarRequest(CriarPedidoRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.NomeCliente))
        {
            throw new ValidationException("O nome do cliente é obrigatório.");
        }

        if (request.NomeCliente.Length > 60)
        {
            throw new ValidationException("O nome do cliente deve ter no máximo 60 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(request.EmailCliente))
        {
            throw new ValidationException("O e-mail do cliente é obrigatório.");
        }

        if (request.EmailCliente.Length > 60)
        {
            throw new ValidationException("O e-mail do cliente deve ter no máximo 60 caracteres.");
        }

        if (!new EmailAddressAttribute().IsValid(request.EmailCliente))
        {
            throw new ValidationException("O e-mail do cliente é inválido.");
        }

        if (request.Itens is null || request.Itens.Count == 0)
        {
            throw new ValidationException("O pedido deve conter pelo menos um item.");
        }

        if (request.Itens.Any(item => item.ProdutoId <= 0))
        {
            throw new ValidationException("Todos os produtos devem ser informados.");
        }

        if (request.Itens.Any(item => item.Quantidade <= 0))
        {
            throw new ValidationException("Todas as quantidades devem ser maiores que zero.");
        }

        var produtoDuplicado = request.Itens
            .GroupBy(item => item.ProdutoId)
            .FirstOrDefault(grupo => grupo.Count() > 1);

        if (produtoDuplicado is not null)
        {
            throw new ValidationException(
                $"O produto {produtoDuplicado.Key} foi informado mais de uma vez.");
        }
    }

    private static PedidoResponse MapearResponse(Pedido pedido)
    {
        return new PedidoResponse
        {
            Id = pedido.Id,
            NomeCliente = pedido.NomeCliente,
            EmailCliente = pedido.EmailCliente,
            Pago = pedido.Pago,
            ValorTotal = pedido.ValorTotal,
            ItensPedido = pedido.ItensPedido
                .Select(item => new ItemPedidoResponse
                {
                    Id = item.Id,
                    IdProduto = item.ProdutoId,
                    NomeProduto = item.Produto.NomeProduto,
                    ValorUnitario = item.ValorUnitario,
                    Quantidade = item.Quantidade
                })
                .ToArray()
        };
    }
}
