namespace Module.Payment.Services.Provider;

/// <summary>Registry of registered payment gateway providers.</summary>
public interface IGatewayRegistry
{
    /// <summary>Gets a registered gateway by its provider key.</summary>
    /// <param name="providerKey">The unique provider key.</param>
    /// <returns>The gateway provider, or NotFound if not registered.</returns>
    Result<IPaymentGatewayActionProvider> GetGateway(string providerKey);

    /// <summary>Checks whether a provider key is registered.</summary>
    /// <param name="providerKey">The provider key to check.</param>
    /// <returns>True if registered.</returns>
    bool IsRegistered(string providerKey);

    /// <summary>Gets the set of registered provider keys.</summary>
    IReadOnlyCollection<string> RegisteredProviders { get; }
}