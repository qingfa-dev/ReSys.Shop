namespace Module.Payment.Services.Models;

public sealed class GatewayProvidersOptions
{
    public const string SectionName = "GatewayProviders";
    public string? SettingsEncryptionKey { get; set; }
}

public sealed class ProviderOptions
{
    public bool Enabled { get; set; } = false;
    public string? SecretKey { get; set; }
    public string? WebhookSecret { get; set; }
    public string? PublishableKey { get; set; }
}