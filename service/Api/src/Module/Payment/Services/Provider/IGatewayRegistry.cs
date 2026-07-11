namespace Module.Payment.Services.Provider;

public interface IGatewayRegistry
{
    Result<IPaymentGatewayActionProvider> GetGateway(string providerKey);
    bool IsRegistered(string providerKey);
    IReadOnlyCollection<string> RegisteredProviders { get; }
}
