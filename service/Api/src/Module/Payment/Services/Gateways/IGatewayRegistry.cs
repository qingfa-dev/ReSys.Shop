namespace Module.Payment.Services.Gateways;

public interface IGatewayRegistry
{
    Result<IPaymentGatewayActionProvider> GetGateway(string providerKey);
    bool IsRegistered(string providerKey);
    IReadOnlyCollection<string> RegisteredProviders { get; }
}
