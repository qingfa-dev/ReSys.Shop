using Microsoft.Extensions.Options;
using Module.Payment.Services.Abstractions;
using Module.Payment.Services.Models;

using Module.Payment.Services.Gateways;

namespace Module.Payment.Services.Gateways;

public sealed class BogusGateway : Gateway
{
    private const long CentsMultiplier = 100;
    private readonly IOptions<BogusSetting> _options;

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

    public BogusGateway(IOptions<BogusSetting> options) { _options = options; }

    public override Task<Result<PaymentGatewayResponse>> PurchaseAsync(
        decimal amount, object? source, GatewayOptions options, CancellationToken ct = default)
        => SimulateGatewayResponse(amount, source, options);

    public override Task<Result<PaymentGatewayResponse>> AuthorizeAsync(
        decimal amount, object? source, GatewayOptions options, CancellationToken ct = default)
        => SimulateGatewayResponse(amount, source, options);

    public override Task<Result<PaymentGatewayResponse>> CaptureAsync(
        decimal amount, string? responseCode, GatewayOptions options, CancellationToken ct = default)
    {
        return Task.FromResult(Result<PaymentGatewayResponse>.Ok(
            new PaymentGatewayResponse(GatewayConstants.Providers.Bogus, authorization: responseCode)));
    }

    public override Task<Result<PaymentGatewayResponse>> VoidAsync(
        string? responseCode, object? source, GatewayOptions options, CancellationToken ct = default)
    {
        return Task.FromResult(Result<PaymentGatewayResponse>.Ok(
            new PaymentGatewayResponse(GatewayConstants.Providers.Bogus, authorization: responseCode)));
    }

    public override Task<Result<PaymentGatewayResponse>> RefundAsync(
        decimal amount, string? responseCode, GatewayOptions options, CancellationToken ct = default)
    {
        return Task.FromResult(Result<PaymentGatewayResponse>.Ok(
            new PaymentGatewayResponse(GatewayConstants.Providers.Bogus, authorization: responseCode)));
    }

    public override Task<Result<PaymentGatewayResponse>> CreateSetupIntentAsync(
        string? customerId, Dictionary<string, string>? metadata, CancellationToken ct = default)
    {
        return Task.FromResult(Result<PaymentGatewayResponse>.Ok(
            new PaymentGatewayResponse(GatewayConstants.Providers.Bogus,
                setupIntentClientSecret: $"{GatewayConstants.Bogus.SetupIntentSecretPrefix}{Guid.NewGuid():N}")));
    }

    private Task<Result<PaymentGatewayResponse>> SimulateGatewayResponse(
        decimal amount, object? source, GatewayOptions options)
    {
        var cardNumber = source as string;
        if (cardNumber == TestCards.Declined)
            return Task.FromResult<Result<PaymentGatewayResponse>>(BogusGatewayResult.Errors.CardDeclined);
        if (cardNumber == TestCards.InsufficientFunds)
            return Task.FromResult<Result<PaymentGatewayResponse>>(BogusGatewayResult.Errors.InsufficientFunds);
        if (cardNumber != TestCards.Success && cardNumber is not null)
            return Task.FromResult<Result<PaymentGatewayResponse>>(BogusGatewayResult.Errors.UnknownCard);

        return Task.FromResult(Result<PaymentGatewayResponse>.Ok(
            new PaymentGatewayResponse(GatewayConstants.Providers.Bogus,
                authorization: $"auth_{Guid.NewGuid():N}")));
    }
}
