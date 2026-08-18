using System.ComponentModel.DataAnnotations;

namespace Pedidos.Application.DTOs.Requests;

public sealed class CriarItemPedidoRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "O produto deve ser informado.")]
    public int ProdutoId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
    public int Quantidade { get; init; }
}
