namespace Module.Payment.Infrastructure.Gateways.Stripe;

public sealed class StripeOptions
{
    public string SecretKey { get; set; } = null!;
    public string? WebhookSecret { get; set; }
    public string? PublishableKey { get; set; }
}
