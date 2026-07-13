namespace Module.Payment.Services.Provider;

// Contract: GetGateway returns Result — NotFound if providerKey not registered
public interface IGatewayRegistry
{
    Result<IPaymentGatewayActionProvider> GetGateway(string providerKey);
    bool IsRegistered(string providerKey);
    IReadOnlyCollection<string> RegisteredProviders { get; }
}