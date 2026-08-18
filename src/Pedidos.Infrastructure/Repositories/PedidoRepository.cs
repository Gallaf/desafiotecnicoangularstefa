using Microsoft.EntityFrameworkCore;
using Pedidos.Application.Interfaces.Persistence;
using Pedidos.Domain.Entities;
using Pedidos.Infrastructure.Persistence;

namespace Pedidos.Infrastructure.Repositories;

public sealed class PedidoRepository(PedidosDbContext dbContext) : IPedidoRepository
{
    public async Task AdicionarAsync(
        Pedido pedido,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Pedidos.AddAsync(pedido, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Pedido?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Pedidos
            .AsNoTracking()
            .Include(pedido => pedido.ItensPedido)
            .ThenInclude(item => item.Produto)
            .SingleOrDefaultAsync(pedido => pedido.Id == id, cancellationToken);
    }
}
