using Microsoft.Extensions.Options;
using Module.Payment.Services.Gateways;
using Stripe;

namespace Module.Payment.Services.Gateways.Stripe;

public sealed class StripeGateway : Gateway
{
    private const long CentsMultiplier = 100;
    private readonly StripeSetting _options;

    public override string ProviderKey => GatewayConstants.Providers.Stripe;
    public override bool AutoCapture => true;
    public override bool SourceRequired => true;
    public override bool PaymentProfilesSupported => true;
    public override bool Supports(object? source) => source is string or null;

    public StripeGateway(IOptions<StripeSetting> options)
    {
        _options = options.Value;
    }

    private RequestOptions BuildRequestOptions(GatewayOptions opt) => new()
    {
        ApiKey = _options.SecretKey,
        IdempotencyKey = opt.IdempotencyKey
    };

    public override async Task<Result<PaymentGatewayResponse>> PurchaseAsync(
        decimal amount, object? source, GatewayOptions options, CancellationToken ct = default)
    {
        try
        {
            var po = CreatePaymentIntentOptions(amount, source, options, autoCapture: true);
            var ro = BuildRequestOptions(options);
            var intent = await new PaymentIntentService().CreateAsync(po, ro, ct).ConfigureAwait(false);
            if (intent.Status != GatewayConstants.Stripe.IntentStatus.Succeeded)
                return Error.BadRequest("Stripe.Purchase.NotSucceeded", $"Purchase status: {intent.Status}");
            return new PaymentGatewayResponse(GatewayConstants.Providers.Stripe,
                authorization: intent.Id);
        }
        catch (StripeException ex) { return MapStripeException(ex); }
    }

    public override async Task<Result<PaymentGatewayResponse>> AuthorizeAsync(
        decimal amount, object? source, GatewayOptions options, CancellationToken ct = default)
    {
        try
        {
            var po = CreatePaymentIntentOptions(amount, source, options, autoCapture: false);
            var ro = BuildRequestOptions(options);
            var intent = await new PaymentIntentService().CreateAsync(po, ro, ct).ConfigureAwait(false);
            if (intent.Status != GatewayConstants.Stripe.IntentStatus.RequiresCapture)
                return Error.BadRequest("Stripe.Authorize.NotRequiresCapture", $"Authorize status: {intent.Status}");
            return new PaymentGatewayResponse(GatewayConstants.Providers.Stripe,
                authorization: intent.Id);
        }
        catch (StripeException ex) { return MapStripeException(ex); }
    }

    public override async Task<Result<PaymentGatewayResponse>> CaptureAsync(
        decimal amount, string? responseCode, GatewayOptions options, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(responseCode))
            return StripeGatewayResult.Errors.CaptureMissingIntent;
        try
        {
            var co = new PaymentIntentCaptureOptions
            {
                AmountToCapture = (long)Math.Round(amount * CentsMultiplier, MidpointRounding.AwayFromZero)
            };
            var ro = BuildRequestOptions(options);
            var intent = await new PaymentIntentService().CaptureAsync(responseCode, co, ro, ct).ConfigureAwait(false);
            return new PaymentGatewayResponse(GatewayConstants.Providers.Stripe, authorization: intent.Id);
        }
        catch (StripeException ex) { return MapStripeException(ex); }
    }

    public override async Task<Result<PaymentGatewayResponse>> VoidAsync(
        string? responseCode, object? source, GatewayOptions options, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(responseCode))
            return StripeGatewayResult.Errors.CancelMissingIntent;
        try
        {
            var co = new PaymentIntentCancelOptions();
            var ro = BuildRequestOptions(options);
            var intent = await new PaymentIntentService().CancelAsync(responseCode, co, ro, ct).ConfigureAwait(false);
            return new PaymentGatewayResponse(GatewayConstants.Providers.Stripe, authorization: intent.Id);
        }
        catch (StripeException ex) { return MapStripeException(ex); }
    }

    public override async Task<Result<PaymentGatewayResponse>> RefundAsync(
        decimal amount, string? responseCode, GatewayOptions options, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(responseCode))
            return StripeGatewayResult.Errors.CreditMissingIntent;
        try
        {
            var ro = new RefundCreateOptions
            {
                PaymentIntent = responseCode,
                Amount = (long)Math.Round(amount * CentsMultiplier, MidpointRounding.AwayFromZero)
            };
            var requestOptions = BuildRequestOptions(options);
            var refund = await new RefundService().CreateAsync(ro, requestOptions, ct).ConfigureAwait(false);
            return new PaymentGatewayResponse(GatewayConstants.Providers.Stripe, authorization: refund.Id);
        }
        catch (StripeException ex) { return MapStripeException(ex); }
    }

    public override async Task<Result<PaymentGatewayResponse>> CreateSetupIntentAsync(
        string? customerId, Dictionary<string, string>? metadata, CancellationToken ct = default)
    {
        try
        {
            var options = new SetupIntentCreateOptions { Metadata = metadata };
            var ro = new RequestOptions { ApiKey = _options.SecretKey };
            var intent = await new SetupIntentService().CreateAsync(options, ro, ct).ConfigureAwait(false);
            return new PaymentGatewayResponse(GatewayConstants.Providers.Stripe,
                setupIntentClientSecret: intent.ClientSecret);
        }
        catch (StripeException ex) { return MapStripeException(ex); }
    }

    public override async Task<string> GetPaymentStatusAsync(
        string paymentIntentId, CancellationToken ct)
    {
        var ro = new RequestOptions { ApiKey = _options.SecretKey };
        var intent = await new PaymentIntentService().GetAsync(paymentIntentId, null, ro, ct);
        return intent.Status;
    }

    private static PaymentIntentCreateOptions CreatePaymentIntentOptions(
        decimal amount, object? source, GatewayOptions options, bool autoCapture)
    {
        var o = new PaymentIntentCreateOptions
        {
            Amount = (long)Math.Round(amount * CentsMultiplier, MidpointRounding.AwayFromZero),
            Currency = GatewayOptions.Currency,
            ConfirmationMethod = GatewayConstants.Stripe.ConfirmationMethod.Manual,
            CaptureMethod = autoCapture
                ? GatewayConstants.Stripe.CaptureMethod.Automatic
                : GatewayConstants.Stripe.CaptureMethod.Manual,
            Metadata = new Dictionary<string, string>
            {
                [GatewayConstants.Metadata.OrderIdKey] = options.OrderId,
                [GatewayConstants.Metadata.PaymentIdKey] = options.PaymentId
            }
        };
        if (source is string s && !string.IsNullOrEmpty(s))
            o.PaymentMethod = s;
        return o;
    }

    private static Result<PaymentGatewayResponse> MapStripeException(StripeException ex)
    {
        var e = ex.StripeError;
        var code = e?.Code ?? GatewayConstants.ErrorCodes.Stripe.UnknownError;
        var msg = e?.DeclineCode is not null
            ? $"Stripe [{code}] decline [{e.DeclineCode}]: {e!.Message}"
            : $"Stripe [{code}]: {e?.Message ?? ex.Message}";
        return Error.BadRequest($"Stripe.{code}", msg);
    }
}
