using System.Net;
using Shared.Application.Models.Results;

namespace Shared.Operational.Webhooks.Domain;

public static class WebhookUrlValidator
{
    private static readonly string[] AllowedSchemes = ["https"];
    private static readonly string[] BlockedHosts = ["127.0.0.1", "0.0.0.0", "169.254.169.254"];
    private static readonly (IPAddress Network, int PrefixLength)[] PrivateRanges =
    [
        (IPAddress.Parse("10.0.0.0"), 8),
        (IPAddress.Parse("172.16.0.0"), 12),
        (IPAddress.Parse("192.168.0.0"), 16),
        (IPAddress.Parse("127.0.0.0"), 8),
        (IPAddress.Parse("169.254.0.0"), 16),
    ];

    private const int MaxUrlLength = 2048;

    public static Result ValidateUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return Error.Validation("Webhooks.Subscription.Url.Empty", "URL must not be empty.");

        if (url.Length > MaxUrlLength)
            return Error.Validation("Webhooks.Subscription.Url.TooLong", $"URL must not exceed {MaxUrlLength} characters.");

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return Error.Validation("Webhooks.Subscription.Url.Invalid", "URL must be a valid absolute URI.");

        if (!AllowedSchemes.Contains(uri.Scheme.ToLowerInvariant()))
            return Error.Validation("Webhooks.Subscription.Url.Scheme", "Only HTTPS URLs are allowed.");

        if (BlockedHosts.Contains(uri.Host))
            return Error.Validation("Webhooks.Subscription.Url.Blocked", "This hostname is not allowed.");

        if (IPAddress.TryParse(uri.Host, out var ip))
        {
            foreach (var (network, prefixLength) in PrivateRanges)
            {
                if (IsInSubnet(ip, network, prefixLength))
                    return Error.Validation("Webhooks.Subscription.Url.Private", "Private network addresses are not allowed.");
            }
        }

        return Result.Ok();
    }

    private static bool IsInSubnet(IPAddress address, IPAddress network, int prefixLength)
    {
        var addressBytes = address.GetAddressBytes();
        var networkBytes = network.GetAddressBytes();
        var maskBytes = new byte[addressBytes.Length];
        var fullBits = prefixLength;

        for (var i = 0; i < maskBytes.Length; i++)
        {
            if (fullBits >= 8)
            {
                maskBytes[i] = 255;
                fullBits -= 8;
            }
            else if (fullBits > 0)
            {
                maskBytes[i] = (byte)(255 << (8 - fullBits));
                fullBits = 0;
            }
        }

        for (var i = 0; i < addressBytes.Length; i++)
        {
            if ((addressBytes[i] & maskBytes[i]) != (networkBytes[i] & maskBytes[i]))
                return false;
        }

        return true;
    }
}
