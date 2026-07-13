namespace Module.Payment.Services.Provider.Stripe;

// Context: Binds from appSettings["GatewayProviders:stripe"] — enables StripeGateway registration
public sealed class StripeSetting
{
    public const string SectionName = GatewayConstants.Configuration.SectionName + ":" + GatewayConstants.Providers.Stripe;

    public bool Enabled { get; set; }
    public string? SecretKey { get; set; }
    public string? WebhookSecret { get; set; }
    public string? PublishableKey { get; set; }
}