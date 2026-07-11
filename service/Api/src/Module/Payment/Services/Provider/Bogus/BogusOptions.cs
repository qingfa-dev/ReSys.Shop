using Module.Payment.Services.Provider;
using Module.Payment.Services.Provider.Bogus;

namespace Module.Payment.Services.Provider.Bogus;

public sealed class BogusSetting
{
    public const string SectionName = GatewayConstants.Configuration.SectionName + ":" + GatewayConstants.Providers.Bogus;

    public bool Enabled { get; set; }
    public string? SecretKey { get; set; }
    public string? WebhookSecret { get; set; }
    public string? PublishableKey { get; set; }
}
