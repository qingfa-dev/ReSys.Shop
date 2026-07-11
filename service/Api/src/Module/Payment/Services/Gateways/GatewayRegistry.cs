using Module.Payment.Services.Abstractions;

namespace Module.Payment.Services.Gateways;

public sealed class GatewayRegistry : IGatewayRegistry
{
    private readonly Dictionary<string, Func<IPaymentGatewayActionProvider>> _gateways = new();

    public IReadOnlyCollection<string> RegisteredProviders => _gateways.Keys;

    public void Register(string providerKey, Func<IPaymentGatewayActionProvider> factory)
    {
        _gateways[providerKey] = factory;
    }

    public Result<IPaymentGatewayActionProvider> GetGateway(string providerKey)
    {
        if (!_gateways.TryGetValue(providerKey, out var factory))
            return Error.NotFound(
                code: $"Gateway.Provider.{providerKey}.NotFound",
                message: $"No gateway registered for provider '{providerKey}'.");

        return new Result<IPaymentGatewayActionProvider>(value: factory());
    }

    public bool IsRegistered(string providerKey) => _gateways.ContainsKey(providerKey);
}
