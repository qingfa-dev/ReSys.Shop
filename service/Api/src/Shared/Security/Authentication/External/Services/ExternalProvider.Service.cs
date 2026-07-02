using Shared.Security.Authentication.External.Models;
using Shared.Security.Authentication.External.Providers;

namespace Shared.Security.Authentication.External.Services;

public sealed class ExternalProviderRegistry(IEnumerable<IExternalLoginProvider> providers)
{
    public PagedResult<ProviderOption> GetAvailableProviders()
    {
        var providerInfos = providers.Select(p => p.GetProviderConfig()).ToList();
        return PagedResult<ProviderOption>.Ok(providerInfos, 1, providers.Count(), providers.Count());
    }
}
