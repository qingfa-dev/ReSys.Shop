using Module.Payment.Services.Provider;
using Module.Payment.Services.Provider.Stripe;

namespace Module.Payment.Services.Provider.Stripe;

public sealed class StripeSetting
{
    public const string SectionName = GatewayConstants.Configuration.SectionName + ":" + GatewayConstants.Providers.Stripe;

    public bool Enabled { get; set; }
    public string? SecretKey { get; set; }
    public string? WebhookSecret { get; set; }
    public string? PublishableKey { get; set; }
}
