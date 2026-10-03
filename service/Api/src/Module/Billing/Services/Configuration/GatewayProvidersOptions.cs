namespace Module.Billing.Services.Configuration;

// Context: Gateway configuration section — binds from appSettings["GatewayProviders"]
public sealed class GatewayProvidersOptions
{
    public const string SectionName = "GatewayProviders";
    public string? SettingsEncryptionKey { get; set; }
}

// Context: Per-provider options — enabled flag gates registration in GatewayRegistry
public sealed class ProviderOptions
{
    public bool Enabled { get; set; } = false;
    public string? SecretKey { get; set; }
    public string? WebhookSecret { get; set; }
    public string? PublishableKey { get; set; }
}