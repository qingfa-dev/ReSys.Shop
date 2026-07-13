namespace Module.Payment.Services.Webhook;

// Contract: HandleAsync returns Result — eventType must be in SupportedEventTypes
public interface IWebhookHandler
{
    string Provider { get; }
    string[] SupportedEventTypes { get; }
    Task<Result> HandleAsync(string eventType, string payload, CancellationToken ct = default);
}