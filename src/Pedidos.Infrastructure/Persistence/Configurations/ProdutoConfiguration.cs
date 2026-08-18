using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pedidos.Domain.Entities;

namespace Pedidos.Infrastructure.Persistence.Configurations;

public sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("Produto");

        builder.HasKey(produto => produto.Id);

        builder.Property(produto => produto.Id)
            .UseIdentityColumn();

        builder.Property(produto => produto.NomeProduto)
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(produto => produto.Valor)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.HasData(
            new { Id = 1, NomeProduto = "Notebook", Valor = 3500.00m },
            new { Id = 2, NomeProduto = "Mouse", Valor = 80.00m },
            new { Id = 3, NomeProduto = "Teclado", Valor = 150.00m });
    }
}
