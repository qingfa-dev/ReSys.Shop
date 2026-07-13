using Microsoft.Extensions.Options;

using Stripe;

namespace Module.Payment.Services.Provider.Stripe;

// Invariant: AutoCapture==true; SourceRequired==true; PaymentProfilesSupported==true
// AgentHint: Add new Stripe API operations as override methods — keep try/catch StripeException pattern
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

    // Build: Gateway request options with API key and idempotency key
    private RequestOptions BuildRequestOptions(GatewayOptions opt) => new()
    {
        ApiKey = _options.SecretKey,
        IdempotencyKey = opt.IdempotencyKey
    };

    // Call: Stripe PaymentIntent.Create with autoCapture=true — succeeds immediately
    // Catch: StripeException → MapStripeException
    public override async Task<Result<PaymentGatewayResponse>> PurchaseAsync(
        decimal amount, object? source, GatewayOptions options, CancellationToken ct = default)
    {
        try
        {
            var po = CreatePaymentIntentOptions(amount, source, options, autoCapture: true);
            var ro = BuildRequestOptions(options);
            var intent = await new PaymentIntentService().CreateAsync(po, ro, ct).ConfigureAwait(false);
            // Check: Intent must be succeeded status for auto-capture
            if (intent.Status != GatewayConstants.Stripe.IntentStatus.Succeeded)
                return Error.BadRequest("Stripe.Purchase.NotSucceeded", $"Purchase status: {intent.Status}");
            return new PaymentGatewayResponse(GatewayConstants.Providers.Stripe,
                authorization: intent.Id,
                clientSecret: intent.ClientSecret);
        }
        catch (StripeException ex) { return MapStripeException(ex); }
    }

    // Call: Stripe PaymentIntent.Create with autoCapture=false — requires separate capture
    // Catch: StripeException → MapStripeException
    public override async Task<Result<PaymentGatewayResponse>> AuthorizeAsync(
        decimal amount, object? source, GatewayOptions options, CancellationToken ct = default)
    {
        try
        {
            var po = CreatePaymentIntentOptions(amount, source, options, autoCapture: false);
            var ro = BuildRequestOptions(options);
            var intent = await new PaymentIntentService().CreateAsync(po, ro, ct).ConfigureAwait(false);
            // Check: Intent must be requires_capture status for manual-capture
            if (intent.Status != GatewayConstants.Stripe.IntentStatus.RequiresCapture)
                return Error.BadRequest("Stripe.Authorize.NotRequiresCapture", $"Authorize status: {intent.Status}");
            return new PaymentGatewayResponse(GatewayConstants.Providers.Stripe,
                authorization: intent.Id,
                clientSecret: intent.ClientSecret);
        }
        catch (StripeException ex) { return MapStripeException(ex); }
    }

    // Call: Stripe PaymentIntent.Capture — amount in cents
    // Catch: StripeException → MapStripeException
    public override async Task<Result<PaymentGatewayResponse>> CaptureAsync(
        decimal amount, string? responseCode, GatewayOptions options, CancellationToken ct = default)
    {
        // Check: ResponseCode (PaymentIntent ID) is required
        if (string.IsNullOrEmpty(responseCode))
            return StripeGatewayResult.Errors.CaptureMissingIntent;
        try
        {
            // Compute: Amount in cents with away-from-zero rounding
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

    // Call: Stripe PaymentIntent.Cancel
    // Catch: StripeException → MapStripeException
    public override async Task<Result<PaymentGatewayResponse>> VoidAsync(
        string? responseCode, object? source, GatewayOptions options, CancellationToken ct = default)
    {
        // Check: ResponseCode (PaymentIntent ID) is required
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

    // Call: Stripe Refund.Create — amount in cents
    // Catch: StripeException → MapStripeException
    public override async Task<Result<PaymentGatewayResponse>> RefundAsync(
        decimal amount, string? responseCode, GatewayOptions options, CancellationToken ct = default)
    {
        // Check: ResponseCode (PaymentIntent ID) is required
        if (string.IsNullOrEmpty(responseCode))
            return StripeGatewayResult.Errors.CreditMissingIntent;
        try
        {
            // Compute: Amount in cents with away-from-zero rounding
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

    // Call: Stripe SetupIntent.Create — for saved payment methods
    // Catch: StripeException → MapStripeException
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

    // Call: Stripe PaymentIntent.Get — returns status string
    public override async Task<string> GetPaymentStatusAsync(
        string paymentIntentId, CancellationToken ct)
    {
        var ro = new RequestOptions { ApiKey = _options.SecretKey };
        var intent = await new PaymentIntentService().GetAsync(paymentIntentId, null, ro, ct);
        return intent.Status;
    }

    // Build: PaymentIntent creation options — amount in cents, metadata, capture method
    private static PaymentIntentCreateOptions CreatePaymentIntentOptions(
        decimal amount, object? source, GatewayOptions options, bool autoCapture)
    {
        // Compute: Amount in cents with away-from-zero rounding
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
        // Assign: Payment method from source string if provided
        if (source is string s && !string.IsNullOrEmpty(s))
            o.PaymentMethod = s;
        return o;
    }

    // Map: StripeException → structured Error response
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