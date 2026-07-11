namespace Module.Payment.Features.Admin.PaymentMethods.Services.Registry;

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
