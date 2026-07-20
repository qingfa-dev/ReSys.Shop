using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace Module.Payment.Services.Provider.Bogus;

// Context: Test-only gateway that simulates Stripe responses without real API calls
// Invariant: AutoCapture==true; PaymentProfilesSupported==false
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

    // Call: Simulated purchase — delegates to SimulateGatewayResponse
    public override Task<Result<PaymentGatewayResponse>> PurchaseAsync(
        decimal amount, object? source, GatewayOptions options, CancellationToken ct = default)
        => SimulateGatewayResponse(amount, source, options);

    // Call: Simulated authorize — delegates to SimulateGatewayResponse
    public override Task<Result<PaymentGatewayResponse>> AuthorizeAsync(
        decimal amount, object? source, GatewayOptions options, CancellationToken ct = default)
        => SimulateGatewayResponse(amount, source, options);

    // Generate: Always succeeds — returns responseCode as authorization
    public override Task<Result<PaymentGatewayResponse>> CaptureAsync(
        decimal amount, string? responseCode, GatewayOptions options, CancellationToken ct = default)
    {
        return Task.FromResult(Result<PaymentGatewayResponse>.Ok(
            new PaymentGatewayResponse(GatewayConstants.Providers.Bogus, authorization: responseCode)));
    }

    // Generate: Always succeeds — returns responseCode as authorization
    public override Task<Result<PaymentGatewayResponse>> VoidAsync(
        string? responseCode, object? source, GatewayOptions options, CancellationToken ct = default)
    {
        return Task.FromResult(Result<PaymentGatewayResponse>.Ok(
            new PaymentGatewayResponse(GatewayConstants.Providers.Bogus, authorization: responseCode)));
    }

    // Generate: Always succeeds — returns responseCode as authorization
    public override Task<Result<PaymentGatewayResponse>> RefundAsync(
        decimal amount, string? responseCode, GatewayOptions options, CancellationToken ct = default)
    {
        return Task.FromResult(Result<PaymentGatewayResponse>.Ok(
            new PaymentGatewayResponse(GatewayConstants.Providers.Bogus, authorization: responseCode)));
    }

    // Generate: Always succeeds — creates fake setup intent secret
    public override Task<Result<PaymentGatewayResponse>> CreateSetupIntentAsync(
        string? customerId, Dictionary<string, string>? metadata, CancellationToken ct = default)
    {
        return Task.FromResult(Result<PaymentGatewayResponse>.Ok(
            new PaymentGatewayResponse(GatewayConstants.Providers.Bogus,
                setupIntentClientSecret: $"{GatewayConstants.Bogus.SetupIntentSecretPrefix}{Guid.NewGuid():N}")));
    }

    private readonly ConcurrentDictionary<string, string> _intentStatuses = new();

    public override Task<string> GetPaymentStatusAsync(string responseCode, CancellationToken ct = default)
    {
        if (_intentStatuses.TryGetValue(responseCode, out var status))
            return Task.FromResult(status);
        return Task.FromResult("unknown");
    }

    // Compute: Simulates gateway response based on test card number
    private Task<Result<PaymentGatewayResponse>> SimulateGatewayResponse(
        decimal amount, object? source, GatewayOptions options)
    {
        var cardNumber = source as string;
        // Check: Known test card numbers map to specific error/success responses
        if (cardNumber == TestCards.Declined)
            return Task.FromResult<Result<PaymentGatewayResponse>>(BogusGatewayResult.Errors.CardDeclined);
        if (cardNumber == TestCards.InsufficientFunds)
            return Task.FromResult<Result<PaymentGatewayResponse>>(BogusGatewayResult.Errors.InsufficientFunds);
        if (cardNumber != TestCards.Success && cardNumber is not null)
            return Task.FromResult<Result<PaymentGatewayResponse>>(BogusGatewayResult.Errors.UnknownCard);

        var authCode = $"auth_{Guid.NewGuid():N}";
        _intentStatuses[authCode] = "succeeded";

        return Task.FromResult(Result<PaymentGatewayResponse>.Ok(
            new PaymentGatewayResponse(GatewayConstants.Providers.Bogus,
                authorization: authCode,
                clientSecret: $"pi_fake_{Guid.NewGuid():N}_secret_{Guid.NewGuid():N}")));
    }
}