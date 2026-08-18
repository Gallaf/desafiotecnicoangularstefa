namespace Pedidos.Domain.Entities;

public sealed class Pedido
{
    private const int NomeClienteMaxLength = 60;
    private const int EmailClienteMaxLength = 60;
    private readonly List<ItemPedido> _itensPedido = [];

    private Pedido()
    {
    }

    public Pedido(
        string nomeCliente,
        string emailCliente,
        DateTime dataCriacao,
        bool pago = false)
    {
        NomeCliente = ValidarTexto(nomeCliente, NomeClienteMaxLength, nameof(nomeCliente));
        EmailCliente = ValidarTexto(emailCliente, EmailClienteMaxLength, nameof(emailCliente));
        DataCriacao = dataCriacao;
        Pago = pago;
    }

    public int Id { get; private set; }

    public string NomeCliente { get; private set; } = string.Empty;

    public string EmailCliente { get; private set; } = string.Empty;

    public DateTime DataCriacao { get; private set; }

    public bool Pago { get; private set; }

    public IReadOnlyCollection<ItemPedido> ItensPedido => _itensPedido;

    public decimal ValorTotal => _itensPedido.Sum(item => item.ValorUnitario * item.Quantidade);

    public ItemPedido AdicionarItem(int produtoId, int quantidade, decimal valorUnitario)
    {
        var item = new ItemPedido(produtoId, quantidade, valorUnitario);
        _itensPedido.Add(item);

        return item;
    }

    public bool RemoverItem(int itemId)
    {
        var item = _itensPedido.FirstOrDefault(itemPedido => itemPedido.Id == itemId);

        return item is not null && _itensPedido.Remove(item);
    }

    private static string ValidarTexto(string valor, int tamanhoMaximo, string nomeParametro)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException("O valor não pode ser vazio.", nomeParametro);
        }

        if (valor.Length > tamanhoMaximo)
        {
            throw new ArgumentException(
                $"O valor deve ter no máximo {tamanhoMaximo} caracteres.",
                nomeParametro);
        }

        return valor;
    }
}
