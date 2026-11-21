using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.Middleware;

/// <summary>
/// Turns unhandled exceptions into a problem details response with the trace id only. The exception
/// message, stack trace and SQL go to the log, never to the client (also in Development).
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>
    /// Status code of a request the client aborted (nginx convention); nobody reads the response.
    /// </summary>
    public const int ClientClosedRequest = 499;

    /// <summary>
    /// Logs the exception and writes the problem details response.
    /// </summary>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        int statusCode;
        switch (exception)
        {
            case OperationCanceledException when httpContext.RequestAborted.IsCancellationRequested:
                logger.LogDebug("Request {Method} {Path} aborted by the client.",
                    httpContext.Request.Method, httpContext.Request.Path);
                httpContext.Response.StatusCode = ClientClosedRequest;
                return true;
            case BadHttpRequestException badRequest:
                // E.g. 413 when the body is over the limit.
                statusCode = badRequest.StatusCode;
                logger.LogWarning("Bad request {Method} {Path}: {StatusCode}.",
                    httpContext.Request.Method, httpContext.Request.Path, statusCode);
                break;
            default:
                statusCode = StatusCodes.Status500InternalServerError;
                logger.LogError(exception, "Unhandled exception for {Method} {Path}.",
                    httpContext.Request.Method, httpContext.Request.Path);
                break;
        }

        httpContext.Response.StatusCode = statusCode;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails { Status = statusCode }
        });
    }
}
