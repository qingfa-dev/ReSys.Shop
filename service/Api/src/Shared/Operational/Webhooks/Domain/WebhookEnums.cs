namespace Shared.Operational.Webhooks.Domain;

public enum WebhookDeliveryStatus
{
    Pending = 0,
    Delivered = 1,
    Failed = 2,
    Dead = 3
}
