

namespace Module.Payment.Services.Provider;

/// <summary>Registry of payment gateway provider factories, keyed by provider identifier.</summary>
public sealed class GatewayRegistry : IGatewayRegistry
{
    private readonly Dictionary<string, Func<IPaymentGatewayActionProvider>> _gateways = new();

    /// <summary>Returns the list of registered provider keys.</summary>
    public IReadOnlyCollection<string> RegisteredProviders => _gateways.Keys;

    /// <summary>Registers a gateway provider factory by key.</summary>
    /// <param name="providerKey">The provider identifier (e.g., "stripe").</param>
    /// <param name="factory">Factory function returning the gateway instance.</param>
    public void Register(string providerKey, Func<IPaymentGatewayActionProvider> factory)
    {
        _gateways[providerKey] = factory;
    }

    /// <summary>Resolves a gateway by provider key. Returns NotFound if unregistered.</summary>
    /// <param name="providerKey">The provider identifier.</param>
    /// <returns>A result containing the gateway instance or a NotFound error.</returns>
    public Result<IPaymentGatewayActionProvider> GetGateway(string providerKey)
    {
        if (!_gateways.TryGetValue(providerKey, out var factory))
            return Error.NotFound(
                code: $"Gateway.Provider.{providerKey}.NotFound",
                message: $"No gateway registered for provider '{providerKey}'.");

        return new Result<IPaymentGatewayActionProvider>(value: factory());
    }

    /// <summary>Checks whether a provider key has been registered.</summary>
    /// <param name="providerKey">The provider identifier.</param>
    /// <returns>True if the provider is registered.</returns>
    public bool IsRegistered(string providerKey) => _gateways.ContainsKey(providerKey);
}