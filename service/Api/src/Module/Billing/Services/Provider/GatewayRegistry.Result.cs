namespace Module.Billing.Services.Provider;

/// <summary>Error factories for gateway registry lookups.</summary>
public static class GatewayRegistryResult
{
    public static class Errors
    {
        public static Error ProviderNotFound(string providerKey) => Error.NotFound(
            code: $"Gateway.Provider.{providerKey}.NotFound",
            message: $"No gateway registered for provider '{providerKey}'.");
    }
}
