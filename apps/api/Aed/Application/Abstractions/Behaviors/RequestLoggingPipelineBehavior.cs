using System.Diagnostics;
using Mediator;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Abstractions.Behaviors;

/// <summary>
/// Pipeline behavior that logs each request with its duration and the error code of a failed result.
/// </summary>
/// <typeparam name="TRequest">Type of the request.</typeparam>
/// <typeparam name="TResponse">Type of the response.</typeparam>
internal sealed class RequestLoggingPipelineBehavior<TRequest, TResponse>(
    ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    /// <summary>
    /// Name of the activity source of the application layer (traced by ServiceDefaults).
    /// </summary>
    public const string ActivitySourceName = "Aed.Application";

    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    /// <summary>
    /// Runs the next handler and logs the outcome.
    /// </summary>
    /// <param name="message">The request.</param>
    /// <param name="next">Next handler in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response of the next handler.</returns>
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        using Activity? activity = ActivitySource.StartActivity(requestName);
        long started = Stopwatch.GetTimestamp();

        TResponse response = await next(message, cancellationToken);

        double elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        if (response is Result { IsFailure: true } failed)
        {
            activity?.SetStatus(ActivityStatusCode.Error, failed.Error.Code);
            logger.LogInformation(
                "Request {RequestName} failed in {ElapsedMs:0} ms with {ErrorCode}.",
                requestName,
                elapsedMs,
                failed.Error.Code);
        }
        else
        {
            logger.LogDebug("Request {RequestName} completed in {ElapsedMs:0} ms.", requestName, elapsedMs);
        }

        return response;
    }
}
