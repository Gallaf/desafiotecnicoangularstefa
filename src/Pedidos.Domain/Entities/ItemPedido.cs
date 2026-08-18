namespace Pedidos.Domain.Entities;

public sealed class ItemPedido
{
    private ItemPedido()
    {
    }

    internal ItemPedido(int produtoId, int quantidade, decimal valorUnitario)
    {
        if (produtoId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(produtoId), "O produto deve ser informado.");
        }

        if (quantidade <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade deve ser maior que zero.");
        }

        if (valorUnitario < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(valorUnitario), "O valor unitário não pode ser negativo.");
        }

        ProdutoId = produtoId;
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
    }

    public int Id { get; private set; }

    public int PedidoId { get; private set; }

    public int ProdutoId { get; private set; }

    public int Quantidade { get; private set; }

    public decimal ValorUnitario { get; private set; }

    public Pedido Pedido { get; private set; } = null!;

    public Produto Produto { get; private set; } = null!;

    public void AtualizarQuantidade(int quantidade)
    {
        if (quantidade <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade deve ser maior que zero.");
        }

        Quantidade = quantidade;
    }
}
