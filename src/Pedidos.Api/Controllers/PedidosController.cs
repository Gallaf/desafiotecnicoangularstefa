using Microsoft.AspNetCore.Mvc;
using Pedidos.Application.DTOs.Requests;
using Pedidos.Application.DTOs.Responses;
using Pedidos.Application.Interfaces;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
public sealed class PedidosController(IPedidoService pedidoService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<PedidoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PedidoResponse>> Criar(
        [FromBody] CriarPedidoRequest request,
        CancellationToken cancellationToken)
    {
        var pedido = await pedidoService.CriarAsync(request, cancellationToken);

        return CreatedAtAction(nameof(ObterPorId), new { id = pedido.Id }, pedido);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<PedidoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PedidoResponse>> ObterPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var pedido = await pedidoService.ObterPorIdAsync(id, cancellationToken);

        if (pedido is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Pedido não encontrado.",
                Detail = $"Não existe pedido com o identificador {id}."
            });
        }

        return Ok(pedido);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<PedidoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PedidoResponse>> Atualizar(
        int id,
        [FromBody] CriarPedidoRequest request,
        CancellationToken cancellationToken)
    {
        var pedido = await pedidoService.AtualizarAsync(id, request, cancellationToken);

        if (pedido is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Pedido não encontrado.",
                Detail = $"Não existe pedido com o identificador {id}."
            });
        }

        return Ok(pedido);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Remover(
        int id,
        CancellationToken cancellationToken)
    {
        var removido = await pedidoService.RemoverAsync(id, cancellationToken);

        if (!removido)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Pedido não encontrado.",
                Detail = $"Não existe pedido com o identificador {id}."
            });
        }

        return NoContent();
    }
}
