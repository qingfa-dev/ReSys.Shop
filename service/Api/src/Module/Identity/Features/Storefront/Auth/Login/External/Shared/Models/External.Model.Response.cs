using Shared.Security.Authentication.External.Models;

namespace Module.Identity.Features.Storefront.Auth.Login.External.Shared.Models;

public abstract record ExternalProvidersBaseResponse
{
    public List<ProviderOption> Providers { get; set; } = [];
}