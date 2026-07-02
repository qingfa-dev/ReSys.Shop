using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Shared.Observability;
using Shared.Observability.Correlation;

namespace Shared.UnitTests.Observability.Correlation;

[Trait("Category", "Unit")]
[Trait("Feature", "Observability")]
public class CorrelationMiddlewareTests
{
    private static (CorrelationMiddleware Middleware, ICorrelationContext Context, HttpContext Http)
        CreateSut(string? headerName = null, string? existingId = null)
    {
        var options = new ObservabilitySetting();
        if (headerName is not null)
            options.CorrelationHeader = headerName;

        var context = new DefaultHttpContext();
        if (existingId is not null)
            context.Request.Headers[options.CorrelationHeader] = existingId;

        var correlationContext = new CorrelationContext();
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ILogger<CorrelationMiddleware>>(NullLogger<CorrelationMiddleware>.Instance)
            .AddSingleton(options)
            .AddSingleton<ICorrelationContext>(correlationContext)
            .BuildServiceProvider();
        context.RequestServices = serviceProvider;

        var middleware = new CorrelationMiddleware(
            next: _ => Task.CompletedTask,
            options: options,
            logger: NullLogger<CorrelationMiddleware>.Instance);

        return (middleware, correlationContext, context);
    }

    [Fact(DisplayName = "When header present should use existing correlation ID")]
    public async Task WhenHeaderPresent_ShouldUseExistingId()
    {
        var (middleware, context, http) = CreateSut(existingId: "abc123");

        await middleware.InvokeAsync(http);

        context.CorrelationId.Should().Be("abc123");
    }

    [Fact(DisplayName = "When header missing should generate new correlation ID")]
    public async Task WhenHeaderMissing_ShouldGenerateNewId()
    {
        var (middleware, context, http) = CreateSut();

        await middleware.InvokeAsync(http);

        context.CorrelationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(context.CorrelationId, out _).Should().BeTrue();
    }

    [Fact(DisplayName = "Should echo correlation ID in response header")]
    public async Task ShouldEchoInResponse()
    {
        var (middleware, _, http) = CreateSut(existingId: "abc123");
        http.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(http);

        http.Response.Headers["X-Correlation-Id"].FirstOrDefault().Should().Be("abc123");
    }

    [Fact(DisplayName = "Should use configured header name")]
    public async Task ShouldUseConfiguredHeaderName()
    {
        var (middleware, context, http) = CreateSut(
            headerName: "X-Trace-Id", existingId: "trace456");
        http.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(http);

        context.CorrelationId.Should().Be("trace456");
        http.Response.Headers["X-Trace-Id"].FirstOrDefault().Should().Be("trace456");
    }
}
