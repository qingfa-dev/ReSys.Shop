namespace Module.Payment.Services.Webhook;

public interface IWebhookHandler
{
    string Provider { get; }
    string[] SupportedEventTypes { get; }
    Task<Result> HandleAsync(string eventType, string payload, CancellationToken ct = default);
}
