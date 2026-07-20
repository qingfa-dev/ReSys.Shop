using System.Net.Mime;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

using Shared.Application.Extensions.Exceptions;

namespace Shared.Application.Exceptions.Handlers;

/// <summary>Global exception handler that catches unhandled exceptions and returns structured JSON error responses.</summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        Loggers.UnhandledException(logger, httpContext.TraceIdentifier, exception);

        var result = Result.Unexpected(
            exception,
            message: "An unexpected error occurred.",
            metadata: ("traceId", httpContext.TraceIdentifier));

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;

        await httpContext.Response.WriteAsJsonAsync(result, cancellationToken);

        return true;
    }

    private static class Loggers
    {
        private static readonly Action<ILogger, string, Exception?> UnhandledExceptionAction =
            LoggerMessage.Define<string>(
                LogLevel.Error,
                new EventId(500, "UnhandledException"),
                "Unhandled exception occurred. TraceId: {TraceId}");

        public static void UnhandledException(ILogger logger, string traceId, Exception exception)
        {
            UnhandledExceptionAction(logger, traceId, exception);
        }
    }
}
