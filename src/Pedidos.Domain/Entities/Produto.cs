namespace Pedidos.Domain.Entities;

public sealed class Produto
{
    private const int NomeProdutoMaxLength = 20;

    private Produto()
    {
    }

    public Produto(string nomeProduto, decimal valor)
    {
        if (string.IsNullOrWhiteSpace(nomeProduto))
        {
            throw new ArgumentException("O nome do produto não pode ser vazio.", nameof(nomeProduto));
        }

        if (nomeProduto.Length > NomeProdutoMaxLength)
        {
            throw new ArgumentException(
                $"O nome do produto deve ter no máximo {NomeProdutoMaxLength} caracteres.",
                nameof(nomeProduto));
        }

        if (valor < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(valor), "O valor não pode ser negativo.");
        }

        NomeProduto = nomeProduto;
        Valor = valor;
    }

    public int Id { get; private set; }

    public string NomeProduto { get; private set; } = string.Empty;

    public decimal Valor { get; private set; }
}
