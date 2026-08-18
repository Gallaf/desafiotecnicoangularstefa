namespace Pedidos.Application.DTOs.Responses;

public sealed class ItemPedidoResponse
{
    public int Id { get; init; }

    public int IdProduto { get; init; }

    public string NomeProduto { get; init; } = string.Empty;

    public decimal ValorUnitario { get; init; }

    public int Quantidade { get; init; }
}
