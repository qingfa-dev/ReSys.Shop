

namespace Module.Payment.Services.Provider;

public sealed class GatewayRegistry : IGatewayRegistry
{
    private readonly Dictionary<string, Func<IPaymentGatewayActionProvider>> _gateways = new();

    public IReadOnlyCollection<string> RegisteredProviders => _gateways.Keys;

    // Add: Register a gateway provider factory by key
    public void Register(string providerKey, Func<IPaymentGatewayActionProvider> factory)
    {
        _gateways[providerKey] = factory;
    }

    // Check: Resolve gateway by provider key — returns NotFound if unregistered
    public Result<IPaymentGatewayActionProvider> GetGateway(string providerKey)
    {
        if (!_gateways.TryGetValue(providerKey, out var factory))
            return Error.NotFound(
                code: $"Gateway.Provider.{providerKey}.NotFound",
                message: $"No gateway registered for provider '{providerKey}'.");

        return new Result<IPaymentGatewayActionProvider>(value: factory());
    }

    // Check: Whether a provider key has been registered
    public bool IsRegistered(string providerKey) => _gateways.ContainsKey(providerKey);
}