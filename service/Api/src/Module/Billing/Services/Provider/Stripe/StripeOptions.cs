namespace Module.Billing.Services.Provider.Stripe;

/// <summary>Stripe gateway configuration, bound from appSettings["GatewayProviders:stripe"].</summary>
public sealed class StripeSetting
{
    public const string SectionName = GatewayConstants.Configuration.SectionName + ":" + GatewayConstants.Providers.Stripe;

    public bool Enabled { get; set; }
    public string? SecretKey { get; set; }
    public string? WebhookSecret { get; set; }
    public string? PublishableKey { get; set; }
}