namespace Module.Payment.Services.Gateways;

public interface IWebhookHandler
{
    string Provider { get; }
    string[] SupportedEventTypes { get; }
    Task<Result> HandleAsync(string eventType, string payload, CancellationToken ct = default);
}
