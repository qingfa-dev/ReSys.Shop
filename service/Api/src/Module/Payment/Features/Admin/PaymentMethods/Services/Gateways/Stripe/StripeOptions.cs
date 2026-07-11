using Module.Payment.Domain.Gateways;

namespace Module.Payment.Features.Admin.PaymentMethods.Services.Gateways.Stripe;

public sealed class StripeSetting
{
    public const string SectionName = GatewayConstants.Configuration.SectionName + ":" + GatewayConstants.Providers.Stripe;

    public bool Enabled { get; set; }
    public string? SecretKey { get; set; }
    public string? WebhookSecret { get; set; }
    public string? PublishableKey { get; set; }
}
