using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pedidos.Domain.Entities;

namespace Pedidos.Infrastructure.Persistence.Configurations;

public sealed class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedido");

        builder.HasKey(pedido => pedido.Id);

        builder.Property(pedido => pedido.Id)
            .UseIdentityColumn();

        builder.Property(pedido => pedido.NomeCliente)
            .HasColumnType("varchar(60)")
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(pedido => pedido.EmailCliente)
            .HasColumnType("varchar(60)")
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(pedido => pedido.DataCriacao)
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(pedido => pedido.Pago)
            .HasColumnType("bit")
            .IsRequired();

        builder.Ignore(pedido => pedido.ValorTotal);

        builder.HasMany(pedido => pedido.ItensPedido)
            .WithOne(item => item.Pedido)
            .HasForeignKey(item => item.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
