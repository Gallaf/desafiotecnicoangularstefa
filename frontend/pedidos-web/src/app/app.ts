import { Component, inject, signal } from '@angular/core';
import { FormArray, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { CriarPedidoRequest, PedidoResponse } from './models/pedido.models';
import { PedidoDetalhes } from './pedido-detalhes/pedido-detalhes';
import { PedidoService } from './services/pedido.service';

type ItemForm = FormGroup<{
  produtoId: FormControl<number | null>;
  quantidade: FormControl<number | null>;
}>;

@Component({
  selector: 'app-root',
  imports: [ReactiveFormsModule, PedidoDetalhes],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  private readonly pedidoService = inject(PedidoService);

  protected readonly pedidoForm = new FormGroup({
    nomeCliente: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(60)],
    }),
    emailCliente: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email, Validators.maxLength(60)],
    }),
    pago: new FormControl(false, { nonNullable: true }),
    itens: new FormArray<ItemForm>([this.criarItemForm()]),
  });

  protected readonly consultaForm = new FormGroup({
    id: new FormControl<number | null>(null, [Validators.required, Validators.min(1)]),
  });

  protected readonly criandoPedido = signal(false);
  protected readonly consultandoPedido = signal(false);
  protected readonly criacaoBemSucedida = signal(false);
  protected readonly mensagemCriacao = signal('');
  protected readonly mensagemConsulta = signal('');
  protected readonly pedidoCriado = signal<PedidoResponse | null>(null);
  protected readonly pedidoConsultado = signal<PedidoResponse | null>(null);

  protected get itens(): FormArray<ItemForm> {
    return this.pedidoForm.controls.itens;
  }

  protected adicionarItem(): void {
    this.itens.push(this.criarItemForm());
  }

  protected removerItem(index: number): void {
    if (this.itens.length > 1) {
      this.itens.removeAt(index);
    }
  }

  protected campoInvalido(campo: 'nomeCliente' | 'emailCliente'): boolean {
    const controle = this.pedidoForm.controls[campo];
    return controle.invalid && controle.touched;
  }

  protected controleItemInvalido(index: number, campo: 'produtoId' | 'quantidade'): boolean {
    const controle = this.itens.at(index).controls[campo];
    return controle.invalid && controle.touched;
  }

  protected criarPedido(): void {
    this.mensagemCriacao.set('');
    this.pedidoCriado.set(null);

    if (this.pedidoForm.invalid) {
      this.pedidoForm.markAllAsTouched();
      this.criacaoBemSucedida.set(false);
      this.mensagemCriacao.set('Revise os campos destacados antes de criar o pedido.');
      return;
    }

    const valor = this.pedidoForm.getRawValue();
    const request: CriarPedidoRequest = {
      nomeCliente: valor.nomeCliente,
      emailCliente: valor.emailCliente,
      pago: valor.pago,
      itens: valor.itens.map((item) => ({
        produtoId: item.produtoId!,
        quantidade: item.quantidade!,
      })),
    };

    this.criandoPedido.set(true);
    this.pedidoService
      .criar(request)
      .pipe(finalize(() => this.criandoPedido.set(false)))
      .subscribe({
        next: (pedido) => {
          this.criacaoBemSucedida.set(true);
          this.mensagemCriacao.set(`Pedido ${pedido.id} criado com sucesso.`);
          this.pedidoCriado.set(pedido);
        },
        error: (erro: unknown) => {
          this.criacaoBemSucedida.set(false);
          this.mensagemCriacao.set(
            this.pedidoService.obterMensagemErro(
              erro,
              'Não foi possível criar o pedido. Tente novamente.',
            ),
          );
        },
      });
  }

  protected consultarPedido(): void {
    this.mensagemConsulta.set('');
    this.pedidoConsultado.set(null);

    if (this.consultaForm.invalid) {
      this.consultaForm.markAllAsTouched();
      return;
    }

    const id = this.consultaForm.controls.id.value!;
    this.consultandoPedido.set(true);
    this.pedidoService
      .obterPorId(id)
      .pipe(finalize(() => this.consultandoPedido.set(false)))
      .subscribe({
        next: (pedido) => this.pedidoConsultado.set(pedido),
        error: (erro: unknown) => {
          this.mensagemConsulta.set(
            this.pedidoService.obterMensagemErro(
              erro,
              'Não foi possível consultar o pedido. Tente novamente.',
            ),
          );
        },
      });
  }

  private criarItemForm(): ItemForm {
    return new FormGroup({
      produtoId: new FormControl<number | null>(null, [Validators.required, Validators.min(1)]),
      quantidade: new FormControl<number | null>(1, [Validators.required, Validators.min(1)]),
    });
  }
}
