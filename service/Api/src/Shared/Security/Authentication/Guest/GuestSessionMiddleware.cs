using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using Shared.Security.Authentication.Guest.Options;

namespace Shared.Security.Authentication.Guest;

public sealed class GuestSessionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly GuestSessionSetting _settings;

    public GuestSessionMiddleware(RequestDelegate next, IOptions<GuestSessionSetting> settings)
    {
        _next = next;
        _settings = settings.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string cookieName = _settings.CookieName;

        if (!context.Request.Cookies.ContainsKey(cookieName))
        {
            string sessionId = Guid.NewGuid().ToString("N");

            CookieOptions cookieOptions = new()
            {
                HttpOnly = _settings.CookieHttpOnly,
                Secure = _settings.CookieSecurePolicy.Equals("Always", StringComparison.OrdinalIgnoreCase),
                SameSite = Enum.Parse<SameSiteMode>(_settings.CookieSameSite, ignoreCase: true),
                MaxAge = TimeSpan.FromDays(_settings.ExpirationInDays)
            };

            context.Response.Cookies.Append(cookieName, sessionId, cookieOptions);
        }

        await _next(context);
    }
}
