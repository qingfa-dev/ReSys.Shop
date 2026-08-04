using Microsoft.Extensions.Options;

using Stripe;

namespace Module.Payment.Services.Provider.Stripe;

// Invariant: AutoCapture==true; SourceRequired==true; PaymentProfilesSupported==true
// AgentHint: Add new Stripe API operations as override methods — keep try/catch StripeException pattern
public sealed class StripeGateway : Gateway
{
    private readonly StripeSetting _options;
    private readonly PaymentIntentService _paymentIntentService = new();
    private readonly RefundService _refundService = new();
    private readonly SetupIntentService _setupIntentService = new();

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

    /// <summary>Creates a PaymentIntent with auto-capture — succeeds immediately.</summary>
    /// <param name="amount">The payment amount.</param>
    /// <param name="source">The payment source identifier (string).</param>
    /// <param name="options">Gateway options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the Stripe PaymentIntent response.</returns>
    public override async Task<Result<PaymentGatewayResponse>> PurchaseAsync(
        decimal amount, object? source, GatewayOptions options, CancellationToken ct = default)
    {
        if (amount > GatewayConstants.Amounts.MaxSafeDollarAmount)
            return StripeGatewayResult.Errors.AmountExceedsMaximum;
        try
        {
            var po = CreatePaymentIntentOptions(amount, source, options, autoCapture: true);
            var ro = BuildRequestOptions(options);
            var intent = await _paymentIntentService.CreateAsync(po, ro, ct).ConfigureAwait(false);
            // Check: Intent status routing
            if (intent.Status == GatewayConstants.Stripe.IntentStatus.Succeeded)
                return new PaymentGatewayResponse(GatewayConstants.Providers.Stripe,
                    authorization: intent.Id,
                    clientSecret: intent.ClientSecret);

            if (intent.Status == GatewayConstants.Stripe.IntentStatus.RequiresAction)
                return new PaymentGatewayResponse(GatewayConstants.Providers.Stripe,
                    authorization: intent.Id,
                    clientSecret: intent.ClientSecret,
                    paymentStatus: GatewayConstants.Stripe.IntentStatus.RequiresAction);

            if (intent.Status == GatewayConstants.Stripe.IntentStatus.RequiresPaymentMethod)
                return StripeGatewayResult.Errors.PaymentMethodRequired(
                    intent.LastPaymentError?.Message);

            return StripeGatewayResult.Errors.PurchaseNotSucceeded(intent.Status);
        }
        catch (StripeException ex) { return MapStripeException(ex); }
    }

    /// <summary>Creates a PaymentIntent with manual capture — requires a separate CaptureAsync call.</summary>
    public override async Task<Result<PaymentGatewayResponse>> AuthorizeAsync(
        decimal amount, object? source, GatewayOptions options, CancellationToken ct = default)
    {
        if (amount > GatewayConstants.Amounts.MaxSafeDollarAmount)
            return StripeGatewayResult.Errors.AmountExceedsMaximum;
        try
        {
            var po = CreatePaymentIntentOptions(amount, source, options, autoCapture: false);
            var ro = BuildRequestOptions(options);
            var intent = await _paymentIntentService.CreateAsync(po, ro, ct).ConfigureAwait(false);
            // Check: Intent must be requires_capture status for manual-capture
            if (intent.Status != GatewayConstants.Stripe.IntentStatus.RequiresCapture)
                return StripeGatewayResult.Errors.AuthorizeNotRequiresCapture(intent.Status);
            return new PaymentGatewayResponse(GatewayConstants.Providers.Stripe,
                authorization: intent.Id,
                clientSecret: intent.ClientSecret);
        }
        catch (StripeException ex) { return MapStripeException(ex); }
    }

    /// <summary>Captures a PaymentIntent — amount converted to cents with away-from-zero rounding.</summary>
    public override async Task<Result<PaymentGatewayResponse>> CaptureAsync(
        decimal amount, string? responseCode, GatewayOptions options, CancellationToken ct = default)
    {
        // Check: ResponseCode (PaymentIntent ID) is required
        if (string.IsNullOrEmpty(responseCode))
            return StripeGatewayResult.Errors.CaptureMissingIntent;
        if (amount > GatewayConstants.Amounts.MaxSafeDollarAmount)
            return StripeGatewayResult.Errors.AmountExceedsMaximum;
        try
        {
            // Compute: Amount in cents with away-from-zero rounding
            var co = new PaymentIntentCaptureOptions
            {
                AmountToCapture = checked((long)Math.Round(amount * GatewayConstants.Amounts.CentsMultiplier, MidpointRounding.AwayFromZero))
            };
            var ro = BuildRequestOptions(options);
            var intent = await _paymentIntentService.CaptureAsync(responseCode, co, ro, ct).ConfigureAwait(false);
            return new PaymentGatewayResponse(GatewayConstants.Providers.Stripe, authorization: intent.Id);
        }
        catch (StripeException ex) { return MapStripeException(ex); }
    }

    /// <summary>Cancels a Stripe PaymentIntent.</summary>
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
            var intent = await _paymentIntentService.CancelAsync(responseCode, co, ro, ct).ConfigureAwait(false);
            return new PaymentGatewayResponse(GatewayConstants.Providers.Stripe, authorization: intent.Id);
        }
        catch (StripeException ex) { return MapStripeException(ex); }
    }

    /// <summary>Creates a Stripe Refund for the given PaymentIntent.</summary>
    public override async Task<Result<PaymentGatewayResponse>> RefundAsync(
        decimal amount, string? responseCode, GatewayOptions options, CancellationToken ct = default)
    {
        // Check: ResponseCode (PaymentIntent ID) is required
        if (string.IsNullOrEmpty(responseCode))
            return StripeGatewayResult.Errors.CreditMissingIntent;
        if (amount > GatewayConstants.Amounts.MaxSafeDollarAmount)
            return StripeGatewayResult.Errors.AmountExceedsMaximum;
        try
        {
            // Compute: Amount in cents with away-from-zero rounding
            var ro = new RefundCreateOptions
            {
                PaymentIntent = responseCode,
                Amount = checked((long)Math.Round(amount * GatewayConstants.Amounts.CentsMultiplier, MidpointRounding.AwayFromZero))
            };
            var requestOptions = BuildRequestOptions(options);
            var refund = await _refundService.CreateAsync(ro, requestOptions, ct).ConfigureAwait(false);
            return new PaymentGatewayResponse(GatewayConstants.Providers.Stripe, authorization: refund.Id);
        }
        catch (StripeException ex) { return MapStripeException(ex); }
    }

    /// <summary>Creates a Stripe SetupIntent for saved payment methods.</summary>
    public override async Task<Result<PaymentGatewayResponse>> CreateSetupIntentAsync(
        string? customerId, Dictionary<string, string>? metadata, CancellationToken ct = default)
    {
        try
        {
            var options = new SetupIntentCreateOptions { Metadata = metadata };
            var ro = new RequestOptions { ApiKey = _options.SecretKey };
            var intent = await _setupIntentService.CreateAsync(options, ro, ct).ConfigureAwait(false);
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
        var intent = await _paymentIntentService.GetAsync(paymentIntentId, null, ro, ct);
        return intent.Status;
    }

    // Build: PaymentIntent creation options — amount in cents, metadata, capture method
    private static PaymentIntentCreateOptions CreatePaymentIntentOptions(
        decimal amount, object? source, GatewayOptions options, bool autoCapture)
    {
        // Compute: Amount in cents with away-from-zero rounding
        var o = new PaymentIntentCreateOptions
        {
            Amount = checked((long)Math.Round(amount * GatewayConstants.Amounts.CentsMultiplier, MidpointRounding.AwayFromZero)),
            Currency = options.Currency,
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
        // Assign: ReturnUrl for 3DS redirect — only when configured
        if (!string.IsNullOrEmpty(options.SuccessUrl))
            o.ReturnUrl = options.SuccessUrl;
        o.PaymentMethodTypes = options.ProviderSpecific is not null
            && options.ProviderSpecific.TryGetValue("payment_method_types", out var types)
            && types is List<string> list
                ? list
                : ["card"];
        // Assign: Statement descriptor suffix — shown on customer card statements
        if (!string.IsNullOrEmpty(options.StatementDescriptorSuffix))
            o.StatementDescriptorSuffix = options.StatementDescriptorSuffix;

        // Assign: Shipping details for fraud detection and card statement context
        if (options.ShippingAddress is not null)
        {
            o.Shipping = new ChargeShippingOptions
            {
                Name = options.ShippingAddress.GetValueOrDefault("name")?.ToString(),
                Address = new AddressOptions
                {
                    Line1 = options.ShippingAddress.GetValueOrDefault("line1")?.ToString(),
                    Line2 = options.ShippingAddress.GetValueOrDefault("line2")?.ToString(),
                    City = options.ShippingAddress.GetValueOrDefault("city")?.ToString(),
                    State = options.ShippingAddress.GetValueOrDefault("state")?.ToString(),
                    PostalCode = options.ShippingAddress.GetValueOrDefault("postal_code")?.ToString(),
                    Country = options.ShippingAddress.GetValueOrDefault("country")?.ToString(),
                }
            };
        }
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

        var isTransient = ex.HttpStatusCode >= System.Net.HttpStatusCode.InternalServerError
            || e?.Type == "api_error"
            || e?.Type == "api_connection_error";

        return isTransient
            ? StripeGatewayResult.Errors.TransientGatewayError(code, msg)
            : StripeGatewayResult.Errors.GatewayError(code, msg);
    }
}