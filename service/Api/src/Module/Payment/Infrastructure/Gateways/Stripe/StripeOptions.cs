using Module.Payment.Domain.Gateways;

namespace Module.Payment.Infrastructure.Gateways.Stripe;

public sealed class StripeOptions
{
    public const string SectionName = GatewayConstants.Configuration.SectionName + ":" + GatewayConstants.Providers.Stripe;

    public bool Enabled { get; set; }
    public string? SecretKey { get; set; }
    public string? WebhookSecret { get; set; }
    public string? PublishableKey { get; set; }
}
