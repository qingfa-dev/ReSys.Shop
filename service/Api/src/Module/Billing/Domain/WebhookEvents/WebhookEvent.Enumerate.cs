namespace Module.Billing.Domain.WebhookEvents;

public enum WebhookEventState
{
    Pending,
    Processing,
    Processed,
    Failed
}
