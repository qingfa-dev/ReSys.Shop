using MediatR;

namespace Shared.Application.Mediators.Behaviours.Exceptions;

/// <summary>
/// Pipeline behavior that catches unhandled exceptions and converts them to Result failures.
/// Prevents exception leakage to the API layer by mapping common exceptions to appropriate Failure codes.
/// </summary>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The type of the response (must inherit from Result).</typeparam>
public sealed partial class ExceptionMappingBehavior<TRequest, TResponse>(
    ILogger<ExceptionMappingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResultRecord
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception ex)
        {
            var requestType = typeof(TRequest).Name;
            var description = $"An unhandled exception occurred while processing {requestType}.";
            if (logger.IsEnabled(LogLevel.Error))
            {
                Loggers.UnhandledException(logger, typeof(TRequest).Name, ex);
                description += $" Exception: {ex.Message}";
            }

            Error failure = Error.Unexpected(
                code: $"{requestType}.Unexpected",
                message: description);

            return (TResponse)(dynamic)failure;
        }
    }
}
