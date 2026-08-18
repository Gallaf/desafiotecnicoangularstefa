using Pedidos.Application.DTOs.Requests;
using Pedidos.Application.DTOs.Responses;

namespace Pedidos.Application.Interfaces;

public interface IPedidoService
{
    Task<PedidoResponse> CriarAsync(
        CriarPedidoRequest request,
        CancellationToken cancellationToken = default);

    Task<PedidoResponse?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<PedidoResponse?> AtualizarAsync(
        int id,
        CriarPedidoRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> RemoverAsync(
        int id,
        CancellationToken cancellationToken = default);
}
