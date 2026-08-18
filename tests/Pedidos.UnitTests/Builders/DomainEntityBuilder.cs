using System.Reflection;
using Pedidos.Domain.Entities;

namespace Pedidos.UnitTests.Builders;

internal static class DomainEntityBuilder
{
    public static Produto CriarProduto(int id, string nomeProduto, decimal valor)
    {
        var produto = new Produto(nomeProduto, valor);
        DefinirPropriedade(produto, nameof(Produto.Id), id);

        return produto;
    }

    public static Pedido CriarPedidoCarregado(
        int pedidoId,
        string nomeCliente,
        string emailCliente,
        bool pago,
        Produto produto,
        int itemId,
        int quantidade,
        decimal valorUnitario)
    {
        var pedido = new Pedido(
            nomeCliente,
            emailCliente,
            new DateTime(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc),
            pago);

        pedido.AdicionarItem(produto.Id, quantidade, valorUnitario);
        MarcarComoPersistido(pedido, pedidoId, produto, itemId);

        return pedido;
    }

    public static void MarcarComoPersistido(
        Pedido pedido,
        int pedidoId,
        Produto produto,
        int primeiroItemId)
    {
        DefinirPropriedade(pedido, nameof(Pedido.Id), pedidoId);

        var item = Assert.Single(pedido.ItensPedido);
        DefinirPropriedade(item, nameof(ItemPedido.Id), primeiroItemId);
        DefinirPropriedade(item, nameof(ItemPedido.PedidoId), pedidoId);
        DefinirPropriedade(item, nameof(ItemPedido.Pedido), pedido);
        DefinirPropriedade(item, nameof(ItemPedido.Produto), produto);
    }

    public static void MarcarItemComoPersistido(
        Pedido pedido,
        ItemPedido item,
        Produto produto,
        int itemId)
    {
        DefinirPropriedade(item, nameof(ItemPedido.Id), itemId);
        DefinirPropriedade(item, nameof(ItemPedido.PedidoId), pedido.Id);
        DefinirPropriedade(item, nameof(ItemPedido.Pedido), pedido);
        DefinirPropriedade(item, nameof(ItemPedido.Produto), produto);
    }

    private static void DefinirPropriedade<T>(T entidade, string nomePropriedade, object valor)
        where T : class
    {
        var propriedade = typeof(T).GetProperty(
            nomePropriedade,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (propriedade is null)
        {
            throw new InvalidOperationException(
                $"A propriedade {typeof(T).Name}.{nomePropriedade} não foi encontrada.");
        }

        propriedade.SetValue(entidade, valor);
    }
}
