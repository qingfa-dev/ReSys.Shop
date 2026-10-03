using Shared.Security.Authentication.External.Models;

namespace Module.Identity.Features.Storefront.Shared.Models;

public abstract record BaseExternalLoginRequest
{
    public string Provider { get; init; } = string.Empty;
    public string IdToken { get; init; } = string.Empty;
}

public abstract record ExternalProvidersBaseResponse
{
    public List<ProviderOption> Providers { get; set; } = [];
}
