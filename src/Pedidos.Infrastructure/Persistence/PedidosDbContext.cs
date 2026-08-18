using Microsoft.EntityFrameworkCore;
using Pedidos.Domain.Entities;

namespace Pedidos.Infrastructure.Persistence;

public sealed class PedidosDbContext(DbContextOptions<PedidosDbContext> options) : DbContext(options)
{
    public DbSet<Pedido> Pedidos => Set<Pedido>();

    public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();

    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PedidosDbContext).Assembly);
    }
}
