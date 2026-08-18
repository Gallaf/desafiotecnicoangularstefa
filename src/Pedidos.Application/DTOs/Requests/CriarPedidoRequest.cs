using System.ComponentModel.DataAnnotations;

namespace Pedidos.Application.DTOs.Requests;

public sealed class CriarPedidoRequest
{
    [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
    [MaxLength(60, ErrorMessage = "O nome do cliente deve ter no máximo 60 caracteres.")]
    public string NomeCliente { get; init; } = string.Empty;

    [Required(ErrorMessage = "O e-mail do cliente é obrigatório.")]
    [MaxLength(60, ErrorMessage = "O e-mail do cliente deve ter no máximo 60 caracteres.")]
    [EmailAddress(ErrorMessage = "O e-mail do cliente é inválido.")]
    public string EmailCliente { get; init; } = string.Empty;

    public bool Pago { get; init; }

    [Required(ErrorMessage = "Os itens do pedido são obrigatórios.")]
    [MinLength(1, ErrorMessage = "O pedido deve conter pelo menos um item.")]
    public IReadOnlyCollection<CriarItemPedidoRequest> Itens { get; init; } = [];
}
