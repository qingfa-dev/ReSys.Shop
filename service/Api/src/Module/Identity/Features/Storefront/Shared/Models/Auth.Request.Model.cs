namespace Module.Identity.Features.Storefront.Shared.Models;

public abstract record BasePasswordLoginRequest
{
    public string Credential { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public abstract record BaseRefreshTokenRequestModel
{
    public string? RefreshToken { get; init; } = null;
}

public abstract record BaseLogOutRequest
{
    public string? RefreshToken { get; init; } = null;
    public bool RevokeAll { get; init; } = false;
}