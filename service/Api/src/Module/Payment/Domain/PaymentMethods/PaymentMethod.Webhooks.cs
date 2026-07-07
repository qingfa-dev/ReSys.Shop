namespace Module.Payment.Domain.PaymentMethods;

// Invariant: WebhookUrl must be a valid absolute URI when set; WebhookSecret must not be empty when WebhookUrl is set
public sealed partial class PaymentMethod
{
    #region Webhook Properties
    public string? WebhookUrl { get; set; }
    public string? WebhookSecret { get; set; }
    public bool WebhookEnabled { get; set; }
    #endregion Webhook Properties
}