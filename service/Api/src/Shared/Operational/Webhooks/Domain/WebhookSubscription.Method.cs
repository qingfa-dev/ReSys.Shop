using Shared.Application.Domain.Concerns.Auditable;

namespace Shared.Operational.Webhooks.Domain;

public static class WebhookSubscriptionMethod
{
    #region Create
    public static Result<WebhookSubscription> Create(
        string @event,
        string url,
        string secretHash,
        int maxRetries = WebhookSubscriptionConstant.Defaults.MaxRetries,
        string? headersJson = null)
    {
        if (string.IsNullOrWhiteSpace(@event))
            return WebhookSubscriptionErrors.Failure.EventRequired;
        if (@event.Length > WebhookSubscriptionConstant.Constraints.Event.MaxLength)
            return WebhookSubscriptionErrors.Failure.EventTooLong;

        if (string.IsNullOrWhiteSpace(url))
            return WebhookSubscriptionErrors.Failure.UrlRequired;
        if (url.Length > WebhookSubscriptionConstant.Constraints.Url.MaxLength)
            return WebhookSubscriptionErrors.Failure.UrlTooLong;

        if (string.IsNullOrWhiteSpace(secretHash))
            return WebhookSubscriptionErrors.Failure.SecretHashRequired;

        var entity = new WebhookSubscription
        {
            Event = @event,
            Url = url,
            SecretHash = secretHash,
            Active = WebhookSubscriptionConstant.Defaults.Active,
            HeadersJson = headersJson,
            MaxRetries = maxRetries,
        };

        AuditableBehavior.Create(entity);
        return entity;
    }
    #endregion

    #region Update
    public static Result<WebhookSubscription> Update(
        this WebhookSubscription entity,
        string? url = default,
        int? maxRetries = default,
        bool? active = default,
        string? headersJson = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        bool isChanged = false;

        if (url is not null)
        {
            if (string.IsNullOrWhiteSpace(url))
                return WebhookSubscriptionErrors.Failure.UrlRequired;
            if (url.Length > WebhookSubscriptionConstant.Constraints.Url.MaxLength)
                return WebhookSubscriptionErrors.Failure.UrlTooLong;

            if (url != entity.Url)
            {
                entity.Url = url;
                isChanged = true;
            }
        }

        if (maxRetries is not null && maxRetries != entity.MaxRetries)
        {
            entity.MaxRetries = maxRetries.Value;
            isChanged = true;
        }

        if (active is not null && active != entity.Active)
        {
            entity.Active = active.Value;
            isChanged = true;
        }

        if (headersJson is not null && headersJson != entity.HeadersJson)
        {
            entity.HeadersJson = headersJson;
            isChanged = true;
        }

        if (isChanged)
            AuditableBehavior.Touch(entity);

        return entity;
    }
    #endregion

    #region Delete
    public static Result<WebhookSubscription> Delete(this WebhookSubscription entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return Result<WebhookSubscription>.NoContent();
    }
    #endregion
}