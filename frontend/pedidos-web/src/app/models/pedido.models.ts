export interface CriarItemPedidoRequest {
  produtoId: number;
  quantidade: number;
}

export interface CriarPedidoRequest {
  nomeCliente: string;
  emailCliente: string;
  pago: boolean;
  itens: CriarItemPedidoRequest[];
}

export interface ItemPedidoResponse {
  id: number;
  idProduto: number;
  nomeProduto: string;
  valorUnitario: number;
  quantidade: number;
}

export interface PedidoResponse {
  id: number;
  nomeCliente: string;
  emailCliente: string;
  pago: boolean;
  valorTotal: number;
  itensPedido: ItemPedidoResponse[];
}

export interface ApiProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}
