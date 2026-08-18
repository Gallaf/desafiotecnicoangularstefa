using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Pedidos.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var isValidationError = exception is ValidationException;
        var statusCode = isValidationError
            ? StatusCodes.Status400BadRequest
            : StatusCodes.Status500InternalServerError;

        if (!isValidationError)
        {
            logger.LogError(exception, "Erro inesperado ao processar a requisição.");
        }

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = isValidationError ? "Requisição inválida." : "Erro interno do servidor.",
                Detail = isValidationError
                    ? exception.Message
                    : "Ocorreu um erro inesperado ao processar a requisição."
            }
        });
    }
}
