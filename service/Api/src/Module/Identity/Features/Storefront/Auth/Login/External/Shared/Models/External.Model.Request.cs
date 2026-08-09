namespace Module.Identity.Features.Shared.Storefront.Auth.Login.External.Shared.Models;

public abstract record BaseExternalLoginRequest
{
    public string Provider { get; init; } = string.Empty;
    public string IdToken { get; init; } = string.Empty;
}