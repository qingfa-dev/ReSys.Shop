using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using Shared.Security.Authentication.Guest.Options;

namespace Shared.Security.Authentication.Contexts.Services;

/// <summary>
/// Implementation of <see cref="ICurrentUser"/> that resolves user information from the HTTP context.
/// </summary>
public class CurrentUser(
    IHttpContextAccessor httpContextAccessor,
    IOptions<GuestSessionSetting> guestSessionSetting) : ICurrentUser
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;
    private HttpContext? HttpContext => httpContextAccessor.HttpContext;

    /// <inheritdoc/>
    public string? UserId => User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <inheritdoc/>
    public string? UserName => User?.FindFirstValue(JwtRegisteredClaimNames.Name) ?? User?.FindFirstValue(ClaimTypes.Name);

    /// <inheritdoc/>
    public string? Email => User?.FindFirstValue(JwtRegisteredClaimNames.Email) ?? User?.FindFirstValue(ClaimTypes.Email);

    /// <inheritdoc/>
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    /// <inheritdoc/>
    public string? IpAddress => HttpContext?.Connection.RemoteIpAddress?.ToString();

    /// <inheritdoc/>
    public string? SessionId => HttpContext?.Request.Cookies[guestSessionSetting.Value.CookieName] ?? HttpContext?.Request.Headers["X-Session-Id"].FirstOrDefault();

    /// <inheritdoc/>
    public string? Device => HttpContext?.Request.Headers.UserAgent.ToString();
}
