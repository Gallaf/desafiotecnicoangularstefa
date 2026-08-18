import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiProblemDetails, CriarPedidoRequest, PedidoResponse } from '../models/pedido.models';

@Injectable({ providedIn: 'root' })
export class PedidoService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiBaseUrl}/api/pedidos`;

  criar(request: CriarPedidoRequest): Observable<PedidoResponse> {
    return this.http.post<PedidoResponse>(this.endpoint, request);
  }

  obterPorId(id: number): Observable<PedidoResponse> {
    return this.http.get<PedidoResponse>(`${this.endpoint}/${id}`);
  }

  obterMensagemErro(erro: unknown, mensagemPadrao: string): string {
    if (!(erro instanceof HttpErrorResponse)) {
      return mensagemPadrao;
    }

    if (erro.status === 404) {
      return 'Pedido não encontrado';
    }

    if (erro.status === 0) {
      return 'Não foi possível conectar à API. Verifique se ela está em execução.';
    }

    const problem = erro.error as ApiProblemDetails | null;
    const errosValidacao = problem?.errors
      ? Object.values(problem.errors).flat().join(' ')
      : '';

    if (erro.status === 400) {
      return errosValidacao || problem?.detail || problem?.title || 'Os dados informados são inválidos.';
    }

    return problem?.detail || mensagemPadrao;
  }
}
