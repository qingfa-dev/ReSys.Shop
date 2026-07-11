using Microsoft.Extensions.Options;
using Module.Payment.Domain.Gateways;

namespace Module.Payment.Infrastructure.Gateways.Bogus;

public sealed class BogusGateway : Gateway
{
    private const long CentsMultiplier = 100;
    private readonly IOptions<BogusOptions> _options;

    public override string ProviderKey => GatewayConstants.Providers.Bogus;
    public override bool AutoCapture => true;
    public override bool SourceRequired => true;
    public override bool PaymentProfilesSupported => false;
    public override bool Supports(object? source) => source is string;

    public static class TestCards
    {
        public const string Success = GatewayConstants.Bogus.TestCards.Success;
        public const string Declined = GatewayConstants.Bogus.TestCards.Declined;
        public const string InsufficientFunds = GatewayConstants.Bogus.TestCards.InsufficientFunds;
    }

    public BogusGateway(IOptions<BogusOptions> options) { _options = options; }

    public override Task<Result<PaymentGatewayResponse>> PurchaseAsync(
        decimal amount, object? source, GatewayOptions options, CancellationToken ct = default)
        => SimulateGatewayResponse(amount, source, options, "purchase");

    public override Task<Result<PaymentGatewayResponse>> AuthorizeAsync(
        decimal amount, object? source, GatewayOptions options, CancellationToken ct = default)
        => SimulateGatewayResponse(amount, source, options, "authorize");

    public override Task<Result<PaymentGatewayResponse>> CaptureAsync(
        decimal amount, string? responseCode, GatewayOptions options, CancellationToken ct = default)
    {
        return Task.FromResult(Result<PaymentGatewayResponse>.Ok(new PaymentGatewayResponse(
            true, GatewayConstants.ResponseMessages.Captured, GatewayConstants.Providers.Bogus,
            authorization: responseCode)));
    }

    public override Task<Result<PaymentGatewayResponse>> VoidAsync(
        string? responseCode, object? source, GatewayOptions options, CancellationToken ct = default)
    {
        return Task.FromResult(Result<PaymentGatewayResponse>.Ok(new PaymentGatewayResponse(
            true, GatewayConstants.ResponseMessages.Voided, GatewayConstants.Providers.Bogus,
            authorization: responseCode)));
    }

    public override Task<Result<PaymentGatewayResponse>> RefundAsync(
        decimal amount, string? responseCode, GatewayOptions options, CancellationToken ct = default)
    {
        return Task.FromResult(Result<PaymentGatewayResponse>.Ok(new PaymentGatewayResponse(
            true, GatewayConstants.ResponseMessages.Refunded, GatewayConstants.Providers.Bogus,
            authorization: responseCode)));
    }

    public override Task<Result<PaymentGatewayResponse>> CreateSetupIntentAsync(
        string? customerId, Dictionary<string, string>? metadata, CancellationToken ct = default)
    {
        return Task.FromResult(Result<PaymentGatewayResponse>.Ok(new PaymentGatewayResponse(
            true, "Bogus setup intent created.", GatewayConstants.Providers.Bogus,
            setupIntentClientSecret: $"{GatewayConstants.Bogus.SetupIntentSecretPrefix}{Guid.NewGuid():N}")));
    }

    private Task<Result<PaymentGatewayResponse>> SimulateGatewayResponse(
        decimal amount, object? source, GatewayOptions options, string action)
    {
        var cardNumber = source as string;
        if (cardNumber == TestCards.Declined)
            return Task.FromResult<Result<PaymentGatewayResponse>>(BogusGatewayResult.Errors.CardDeclined);
        if (cardNumber == TestCards.InsufficientFunds)
            return Task.FromResult<Result<PaymentGatewayResponse>>(BogusGatewayResult.Errors.InsufficientFunds);
        if (cardNumber != TestCards.Success && cardNumber is not null)
            return Task.FromResult<Result<PaymentGatewayResponse>>(BogusGatewayResult.Errors.UnknownCard);

        return Task.FromResult(Result<PaymentGatewayResponse>.Ok(new PaymentGatewayResponse(
            true, $"{action} captured.", GatewayConstants.Providers.Bogus,
            authorization: $"auth_{Guid.NewGuid():N}")));
    }
}
