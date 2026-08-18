using Microsoft.EntityFrameworkCore;
using Pedidos.Application.Interfaces.Persistence;
using Pedidos.Domain.Entities;
using Pedidos.Infrastructure.Persistence;

namespace Pedidos.Infrastructure.Repositories;

public sealed class ProdutoRepository(PedidosDbContext dbContext) : IProdutoRepository
{
    public async Task<IReadOnlyCollection<Produto>> ObterPorIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Produtos
            .AsNoTracking()
            .Where(produto => ids.Contains(produto.Id))
            .ToArrayAsync(cancellationToken);
    }
}
