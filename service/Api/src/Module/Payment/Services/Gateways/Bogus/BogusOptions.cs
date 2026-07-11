using Module.Payment.Services.Gateways;

namespace Module.Payment.Services.Gateways.Bogus;

public sealed class BogusSetting
{
    public const string SectionName = GatewayConstants.Configuration.SectionName + ":" + GatewayConstants.Providers.Bogus;

    public bool Enabled { get; set; }
    public string? SecretKey { get; set; }
    public string? WebhookSecret { get; set; }
    public string? PublishableKey { get; set; }
}
