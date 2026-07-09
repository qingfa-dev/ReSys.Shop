namespace Shared.Operational.Webhooks.Domain;

public static class WebhookDeliveryMethod
{
    #region Create
    public static Result<WebhookDelivery> Create(
        Guid subscriptionId,
        string @event,
        string payloadJson)
    {
        if (subscriptionId == Guid.Empty)
            return WebhookDeliveryResult.Failure.SubscriptionRequired;

        if (string.IsNullOrWhiteSpace(@event))
            return WebhookDeliveryResult.Failure.EventRequired;

        if (string.IsNullOrWhiteSpace(payloadJson))
            return WebhookDeliveryResult.Failure.PayloadRequired;

        var entity = new WebhookDelivery
        {
            SubscriptionId = subscriptionId,
            Event = @event,
            PayloadJson = payloadJson,
            Status = WebhookDeliveryStatus.Pending,
            AttemptCount = 0,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };

        return entity;
    }
    #endregion

    #region MarkDelivered
    public static Result<WebhookDelivery> MarkDelivered(this WebhookDelivery delivery)
    {
        ArgumentNullException.ThrowIfNull(delivery);

        delivery.Status = WebhookDeliveryStatus.Delivered;
        delivery.DeliveredAtUtc = DateTimeOffset.UtcNow;
        return delivery;
    }
    #endregion

    #region MarkFailed
    public static Result<WebhookDelivery> MarkFailed(
        this WebhookDelivery delivery,
        string error,
        int maxRetries = WebhookDeliveryConstant.Defaults.MaxRetries)
    {
        ArgumentNullException.ThrowIfNull(delivery);

        delivery.LastError = error;
        delivery.AttemptCount += 1;

        if (delivery.AttemptCount >= maxRetries)
        {
            delivery.Status = WebhookDeliveryStatus.Dead;
        }
        else
        {
            delivery.Status = WebhookDeliveryStatus.Failed;
            var delaySeconds = (int)Math.Pow(
                WebhookDeliveryConstant.Defaults.RetryDelayExponentBase,
                delivery.AttemptCount)
                * WebhookDeliveryConstant.Defaults.RetryDelayBaseSeconds;
            delivery.NextRetryAtUtc = DateTimeOffset.UtcNow.AddSeconds(delaySeconds);
        }

        return delivery;
    }
    #endregion

    #region MarkDead
    public static Result<WebhookDelivery> MarkDead(this WebhookDelivery delivery, string error)
    {
        ArgumentNullException.ThrowIfNull(delivery);

        delivery.Status = WebhookDeliveryStatus.Dead;
        delivery.LastError = error;
        return delivery;
    }
    #endregion
}