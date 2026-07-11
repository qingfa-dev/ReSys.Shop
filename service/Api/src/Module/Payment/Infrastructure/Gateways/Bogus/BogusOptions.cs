using Module.Payment.Domain.Gateways;

namespace Module.Payment.Infrastructure.Gateways.Bogus;

public sealed class BogusOptions
{
    public const string SectionName = GatewayConstants.Configuration.SectionName + ":" + GatewayConstants.Providers.Bogus;

    public bool Enabled { get; set; }
    public string? SecretKey { get; set; }
    public string? WebhookSecret { get; set; }
    public string? PublishableKey { get; set; }
}
