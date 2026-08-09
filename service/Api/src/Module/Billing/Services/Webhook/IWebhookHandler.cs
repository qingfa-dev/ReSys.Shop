namespace Module.Billing.Services.Webhook;

/// <summary>Handles inbound webhook events for a specific payment provider.</summary>
public interface IWebhookHandler
{
    /// <summary>The payment provider key (e.g., "stripe").</summary>
    string Provider { get; }
    /// <summary>The event types this handler supports.</summary>
    string[] SupportedEventTypes { get; }
    /// <summary>Processes an inbound webhook event.</summary>
    Task<Result> HandleAsync(string eventType, string payload, CancellationToken ct = default);
}