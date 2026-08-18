using Pedidos.Domain.Entities;

namespace Pedidos.Application.Interfaces.Persistence;

public interface IProdutoRepository
{
    Task<IReadOnlyCollection<Produto>> ObterPorIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default);
}
