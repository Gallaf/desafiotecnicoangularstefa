import { CurrencyPipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { PedidoResponse } from '../models/pedido.models';

@Component({
  selector: 'app-pedido-detalhes',
  imports: [CurrencyPipe],
  templateUrl: './pedido-detalhes.html',
  styleUrl: './pedido-detalhes.css',
})
export class PedidoDetalhes {
  readonly pedido = input.required<PedidoResponse>();
}
