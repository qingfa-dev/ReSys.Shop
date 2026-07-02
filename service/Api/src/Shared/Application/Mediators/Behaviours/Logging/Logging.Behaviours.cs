using MediatR;

namespace Shared.Application.Mediators.Behaviours.Logging;

/// <summary>
/// Pipeline behavior that logs request start, completion, and failure using high-performance LoggerMessage.
/// Logs request type, outcome (success/failure), and any error details.
/// </summary>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The type of the response (must inherit from Result).</typeparam>
public sealed partial class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResultRecord
{
    // Boundary: Application → Infrastructure — logging pipeline cross-cuts all requests

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Loggers.HandlingRequest(logger, typeof(TRequest).Name);

        TResponse response = await next(cancellationToken);

        if (!response.IsSuccess)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                Loggers.RequestFailed(
                    logger,
                    typeof(TRequest).Name,
                    string.Join(", ", response.Errors.Select(f => f.Message)));
            }
        }
        else
        {
            Loggers.RequestSucceeded(logger, typeof(TRequest).Name);
        }

        return response;
    }
}
