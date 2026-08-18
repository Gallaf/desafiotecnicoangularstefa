using Pedidos.Domain.Entities;

namespace Pedidos.Application.Interfaces.Persistence;

public interface IPedidoRepository
{
    Task AdicionarAsync(Pedido pedido, CancellationToken cancellationToken = default);

    Task<Pedido?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
}
