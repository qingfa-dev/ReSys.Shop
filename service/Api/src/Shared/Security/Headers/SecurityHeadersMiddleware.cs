using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using Shared.Security.Headers.Options;

namespace Shared.Security.Headers;

/// <summary>Adds security headers (X-Content-Type-Options, X-Frame-Options, Content-Security-Policy, Referrer-Policy, Permissions-Policy) to every HTTP response. HSTS should be handled by the reverse proxy (Aspire/nginx) in production.</summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityHeadersSetting _settings;

    public SecurityHeadersMiddleware(RequestDelegate next, IOptions<SecurityHeadersSetting> settings)
    {
        _next = next;
        _settings = settings.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_settings.IsEnabled)
        {
            await _next(context);
            return;
        }

        IHeaderDictionary headers = context.Response.Headers;

        AppendIfSet(headers, SecurityHeadersSettingConstant.Keys.XContentTypeOptions, _settings.XContentTypeOptions);
        AppendIfSet(headers, SecurityHeadersSettingConstant.Keys.XFrameOptions, _settings.XFrameOptions);
        AppendIfSet(headers, SecurityHeadersSettingConstant.Keys.ContentSecurityPolicy, _settings.ContentSecurityPolicy);
        AppendIfSet(headers, SecurityHeadersSettingConstant.Keys.ReferrerPolicy, _settings.ReferrerPolicy);
        AppendIfSet(headers, SecurityHeadersSettingConstant.Keys.PermissionsPolicy, _settings.PermissionsPolicy);

        await _next(context);
    }

    private static void AppendIfSet(IHeaderDictionary headers, string name, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            headers.Append(name, value);
        }
    }
}
