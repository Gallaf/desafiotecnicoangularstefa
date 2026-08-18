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

    public Task<Pedido?> ObterParaAtualizacaoAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Pedidos
            .Include(pedido => pedido.ItensPedido)
            .ThenInclude(item => item.Produto)
            .SingleOrDefaultAsync(pedido => pedido.Id == id, cancellationToken);
    }

    public async Task AtualizarAsync(
        Pedido pedido,
        CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> RemoverAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var pedido = await dbContext.Pedidos
            .SingleOrDefaultAsync(pedido => pedido.Id == id, cancellationToken);

        if (pedido is null)
        {
            return false;
        }

        dbContext.Pedidos.Remove(pedido);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
