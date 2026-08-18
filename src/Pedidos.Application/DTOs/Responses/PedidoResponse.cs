namespace Pedidos.Application.DTOs.Responses;

public sealed class PedidoResponse
{
    public int Id { get; init; }

    public string NomeCliente { get; init; } = string.Empty;

    public string EmailCliente { get; init; } = string.Empty;

    public bool Pago { get; init; }

    public decimal ValorTotal { get; init; }

    public IReadOnlyCollection<ItemPedidoResponse> ItensPedido { get; init; } = [];
}
