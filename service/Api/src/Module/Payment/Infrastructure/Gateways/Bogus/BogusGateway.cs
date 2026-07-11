using Microsoft.Extensions.Options;

using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.PaymentCaptures;

using PaymentRecord = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.Payment.Infrastructure.Gateways.Bogus;

/// <summary>
/// Offline payment gateway for local development. No external network calls.
/// Implements the contract of <see cref="Gateway"/> using deterministic test card numbers.
/// </summary>
public sealed class BogusGateway : Gateway
{
    private const long CentsMultiplier = 100;

    public static class TestCards
    {
        public const string Success = "4242424242424242";
        public const string Declined = "4000000000000002";
        public const string InsufficientFunds = "4000000000009995";
    }

    public BogusGateway(IOptions<BogusOptions> options) { }

    public override bool AutoCapture => true;
    public override bool SourceRequired => true;
    public override bool PaymentProfilesSupported => false;
    public override bool Supports(object? source) => source is string;

    public override Task<Result<PaymentGatewayResponse>> PurchaseAsync(
        decimal amountInCents, object? source, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        var card = source as string;
        return Task.FromResult(ProcessCard(card, "captured"));
    }

    public override Task<Result<PaymentGatewayResponse>> AuthorizeAsync(
        decimal amountInCents, object? source, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        var card = source as string;
        return Task.FromResult(ProcessCard(card, "authorized"));
    }

    public override Task<Result<PaymentGatewayResponse>> CaptureAsync(
        decimal amount, string? responseCode, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<PaymentGatewayResponse>.Ok(
            new PaymentGatewayResponse(true, "Captured.", authorization: responseCode ?? Guid.NewGuid().ToString())));
    }

    public override Task<Result<PaymentGatewayResponse>> VoidAsync(
        string? responseCode, object? source, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<PaymentGatewayResponse>.Ok(
            new PaymentGatewayResponse(true, "Voided.", authorization: responseCode)));
    }

    public override Task<Result<PaymentGatewayResponse>> CancelAsync(
        string? responseCode, object? payment, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<PaymentGatewayResponse>.Ok(
            new PaymentGatewayResponse(true, "Cancelled.", authorization: responseCode)));
    }

    public override Task<Result<PaymentGatewayResponse>> CreditAsync(
        decimal amount, string? responseCode, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<PaymentGatewayResponse>.Ok(
            new PaymentGatewayResponse(true, "Refunded.", authorization: responseCode)));
    }

    private static Result<PaymentGatewayResponse> ProcessCard(string? card, string verb)
    {
        var auth = Guid.NewGuid().ToString();
        return card switch
        {
            TestCards.Success => new PaymentGatewayResponse(true, $"Payment {verb}.", authorization: auth),
            TestCards.Declined => BogusGatewayResult.Errors.CardDeclined,
            TestCards.InsufficientFunds => BogusGatewayResult.Errors.InsufficientFunds,
            _ => BogusGatewayResult.Errors.UnknownCard
        };
    }
}
