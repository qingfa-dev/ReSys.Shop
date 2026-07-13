// Context: Legacy duplicate of Services.Webhook.IWebhookHandler
namespace Module.Payment.Services.Abstractions;

public interface IWebhookHandler
{
    string Provider { get; }
    string[] SupportedEventTypes { get; }
    Task<Result> HandleAsync(string eventType, string payload, CancellationToken ct = default);
}