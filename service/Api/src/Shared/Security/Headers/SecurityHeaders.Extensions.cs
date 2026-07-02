using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Shared.Security.Headers.Options;

namespace Shared.Security.Headers;

public static class SecurityHeadersExtensions
{
    public static WebApplicationBuilder AddSecurityHeaders(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<SecurityHeadersSetting>();

        return builder;
    }

    public static WebApplication UseSecurityHeaders(this WebApplication app)
    {
        app.UseMiddleware<SecurityHeadersMiddleware>();

        return app;
    }
}
