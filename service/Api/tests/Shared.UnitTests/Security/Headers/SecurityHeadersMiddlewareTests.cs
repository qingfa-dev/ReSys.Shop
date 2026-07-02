using Microsoft.AspNetCore.Http;

using Shared.Security.Headers;
using Shared.Security.Headers.Options;

namespace Shared.UnitTests.Security.Headers;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "SecurityHeaders")]
public sealed class SecurityHeadersMiddlewareTests
{
    private static SecurityHeadersMiddleware CreateMiddleware(SecurityHeadersSetting settings, RequestDelegate? next = null)
    {
        next ??= static _ => Task.CompletedTask;

        return new SecurityHeadersMiddleware(next, Microsoft.Extensions.Options.Options.Create(settings));
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        return new DefaultHttpContext();
    }

    private static string GetHeader(IHeaderDictionary headers, string name)
    {
        return headers[name].ToString();
    }

    [Fact(DisplayName = "Should append X-Content-Type-Options by default")]
    public async Task InvokeAsync_ShouldAppendXContentTypeOptions()
    {
        var settings = new SecurityHeadersSetting();
        var middleware = CreateMiddleware(settings);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        GetHeader(context.Response.Headers, "X-Content-Type-Options")
            .Should().Be("nosniff");
    }

    [Fact(DisplayName = "Should append X-Frame-Options by default")]
    public async Task InvokeAsync_ShouldAppendXFrameOptions()
    {
        var settings = new SecurityHeadersSetting();
        var middleware = CreateMiddleware(settings);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        GetHeader(context.Response.Headers, "X-Frame-Options")
            .Should().Be("DENY");
    }

    [Fact(DisplayName = "Should append Referrer-Policy by default")]
    public async Task InvokeAsync_ShouldAppendReferrerPolicy()
    {
        var settings = new SecurityHeadersSetting();
        var middleware = CreateMiddleware(settings);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        GetHeader(context.Response.Headers, "Referrer-Policy")
            .Should().Be("strict-origin-when-cross-origin");
    }

    [Fact(DisplayName = "Should append Permissions-Policy by default")]
    public async Task InvokeAsync_ShouldAppendPermissionsPolicy()
    {
        var settings = new SecurityHeadersSetting();
        var middleware = CreateMiddleware(settings);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        GetHeader(context.Response.Headers, "Permissions-Policy")
            .Should().Be("camera=(), microphone=(), geolocation=()");
    }

    [Fact(DisplayName = "Should append Content-Security-Policy when configured")]
    public async Task InvokeAsync_WithCsp_ShouldAppend()
    {
        var settings = new SecurityHeadersSetting
        {
            ContentSecurityPolicy = "default-src 'self'"
        };

        var middleware = CreateMiddleware(settings);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        GetHeader(context.Response.Headers, "Content-Security-Policy")
            .Should().Be("default-src 'self'");
    }

    [Fact(DisplayName = "Should not append headers when IsEnabled is false")]
    public async Task InvokeAsync_WhenDisabled_ShouldNotAppendHeaders()
    {
        var settings = new SecurityHeadersSetting
        {
            IsEnabled = false,
            XContentTypeOptions = "nosniff"
        };

        var middleware = CreateMiddleware(settings);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.Headers.Should()
            .NotContain(kvp => kvp.Key == "X-Content-Type-Options");
    }

    [Fact(DisplayName = "Should not append empty header values")]
    public async Task InvokeAsync_WithEmptyValue_ShouldNotAppend()
    {
        var settings = new SecurityHeadersSetting
        {
            PermissionsPolicy = string.Empty
        };

        var middleware = CreateMiddleware(settings);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.Headers.Should()
            .NotContain(kvp => kvp.Key == "Permissions-Policy");
    }

    [Fact(DisplayName = "Should call the next delegate")]
    public async Task InvokeAsync_ShouldCallNext()
    {
        bool nextCalled = false;

        var middleware = CreateMiddleware(new SecurityHeadersSetting(), ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = CreateHttpContext();
        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }
}
