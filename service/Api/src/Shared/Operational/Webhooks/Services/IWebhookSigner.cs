namespace Shared.Operational.Webhooks.Services;

public interface IWebhookSigner
{
    string Sign(string payload, string secret);
}
