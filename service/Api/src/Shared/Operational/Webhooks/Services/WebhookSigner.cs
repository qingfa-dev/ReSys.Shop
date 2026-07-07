using System.Security.Cryptography;
using System.Text;

namespace Shared.Operational.Webhooks.Services;

public sealed class WebhookSigner : IWebhookSigner
{
    public string Sign(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
