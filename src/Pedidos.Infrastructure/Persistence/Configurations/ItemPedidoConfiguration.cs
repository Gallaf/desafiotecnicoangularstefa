using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pedidos.Domain.Entities;

namespace Pedidos.Infrastructure.Persistence.Configurations;

public sealed class ItemPedidoConfiguration : IEntityTypeConfiguration<ItemPedido>
{
    public void Configure(EntityTypeBuilder<ItemPedido> builder)
    {
        builder.ToTable("ItensPedido");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .UseIdentityColumn();

        builder.Property(item => item.PedidoId)
            .HasColumnName("IdPedido")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(item => item.ProdutoId)
            .HasColumnName("IdProduto")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(item => item.Quantidade)
            .HasColumnType("int")
            .IsRequired();

        builder.Property(item => item.ValorUnitario)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.HasOne(item => item.Produto)
            .WithMany()
            .HasForeignKey(item => item.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
