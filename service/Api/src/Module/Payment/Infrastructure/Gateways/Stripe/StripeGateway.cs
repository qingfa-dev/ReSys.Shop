using Microsoft.Extensions.Options;
using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.Payments;
using Stripe;

namespace Module.Payment.Infrastructure.Gateways.Stripe;

public sealed class StripeGateway : Gateway
{
    private const long CentsMultiplier = 100;
    private readonly StripeOptions _options;

    public StripeGateway(IOptions<StripeOptions> options)
    {
        _options = options.Value;
        StripeConfiguration.ApiKey = _options.SecretKey;
    }

    public override bool AutoCapture => true;
    public override bool SourceRequired => true;
    public override bool PaymentProfilesSupported => true;
    public override bool Supports(object? source) => source is string or null;

    public override async Task<Result<PaymentGatewayResponse>> PurchaseAsync(decimal amountInCents, object? source, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            var po = CreatePaymentIntentOptions(amountInCents, source, options, autoCapture: true);
            var requestOptions = new RequestOptions { IdempotencyKey = options.IdempotencyKey ?? Guid.NewGuid().ToString() };
            var intent = await new PaymentIntentService().CreateAsync(po, requestOptions, cancellationToken).ConfigureAwait(false);
            return new PaymentGatewayResponse(true, intent.Status == "succeeded" ? "Payment captured." : $"Status: {intent.Status}", authorization: intent.Id);
        }
        catch (StripeException ex) { return MapStripeException(ex); }
    }

    public override async Task<Result<PaymentGatewayResponse>> AuthorizeAsync(decimal amountInCents, object? source, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            var po = CreatePaymentIntentOptions(amountInCents, source, options, autoCapture: false);
            var requestOptions = new RequestOptions { IdempotencyKey = options.IdempotencyKey ?? Guid.NewGuid().ToString() };
            var intent = await new PaymentIntentService().CreateAsync(po, requestOptions, cancellationToken).ConfigureAwait(false);
            return new PaymentGatewayResponse(true, intent.Status == "requires_capture" ? "Authorized." : $"Status: {intent.Status}", authorization: intent.Id);
        }
        catch (StripeException ex) { return MapStripeException(ex); }
    }

    public override async Task<Result<PaymentGatewayResponse>> CaptureAsync(decimal amount, string? responseCode, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(responseCode)) return StripeGatewayResult.Errors.CaptureMissingIntent;
        try
        {
            var cap = new PaymentIntentCaptureOptions { AmountToCapture = (long)Math.Round(amount * CentsMultiplier, MidpointRounding.AwayFromZero) };
            var requestOptions = new RequestOptions { IdempotencyKey = options.IdempotencyKey ?? Guid.NewGuid().ToString() };
            var intent = await new PaymentIntentService().CaptureAsync(responseCode, cap, requestOptions, cancellationToken).ConfigureAwait(false);
            return new PaymentGatewayResponse(true, "Captured.", authorization: intent.Id);
        }
        catch (StripeException ex) { return MapStripeException(ex); }
    }

    public override Task<Result<PaymentGatewayResponse>> VoidAsync(string? responseCode, object? source, GatewayOptions options, CancellationToken cancellationToken = default) => CancelPaymentIntentAsync(responseCode, options, cancellationToken);

    public override Task<Result<PaymentGatewayResponse>> CancelAsync(string? responseCode, object? payment, CancellationToken cancellationToken = default) => CancelPaymentIntentAsync(responseCode, null, cancellationToken);

    public override async Task<Result<PaymentGatewayResponse>> CreditAsync(decimal amount, string? responseCode, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(responseCode)) return StripeGatewayResult.Errors.CreditMissingIntent;
        try
        {
            var ro = new RefundCreateOptions { PaymentIntent = responseCode, Amount = (long)Math.Round(amount * CentsMultiplier, MidpointRounding.AwayFromZero) };
            var requestOptions = new RequestOptions { IdempotencyKey = options.IdempotencyKey ?? Guid.NewGuid().ToString() };
            var refund = await new RefundService().CreateAsync(ro, requestOptions, cancellationToken).ConfigureAwait(false);
            return new PaymentGatewayResponse(true, "Refunded.", authorization: refund.Id);
        }
        catch (StripeException ex) { return MapStripeException(ex); }
    }

    public override async Task<string> GetPaymentIntentStatusAsync(string paymentIntentId, CancellationToken ct)
    {
        var requestOptions = new RequestOptions { IdempotencyKey = Guid.NewGuid().ToString() };
        var intent = await new PaymentIntentService().GetAsync(paymentIntentId, null, requestOptions, ct);
        return intent.Status;
    }

    private static PaymentIntentCreateOptions CreatePaymentIntentOptions(decimal amountInCents, object? source, GatewayOptions options, bool autoCapture)
    {
        var o = new PaymentIntentCreateOptions
        {
            Amount = (long)Math.Round(amountInCents * CentsMultiplier, MidpointRounding.AwayFromZero),
            Currency = GatewayOptions.Currency.ToLowerInvariant(),
            ConfirmationMethod = "manual",
            CaptureMethod = autoCapture ? "automatic" : "manual",
            Metadata = new Dictionary<string, string> { ["order_id"] = options.OrderId ?? "", ["payment_id"] = options.PaymentId ?? "" }
        };
        if (source is string s && !string.IsNullOrEmpty(s)) o.PaymentMethod = s;
        return o;
    }

    private static async Task<Result<PaymentGatewayResponse>> CancelPaymentIntentAsync(string? responseCode, GatewayOptions? options, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(responseCode)) return StripeGatewayResult.Errors.CancelMissingIntent;
        try
        {
            var co = new PaymentIntentCancelOptions();
            var requestOptions = new RequestOptions { IdempotencyKey = options?.IdempotencyKey ?? Guid.NewGuid().ToString() };
            var intent = await new PaymentIntentService().CancelAsync(responseCode, co, requestOptions, cancellationToken).ConfigureAwait(false);
            return new PaymentGatewayResponse(true, "Voided.", authorization: intent.Id);
        }
        catch (StripeException ex) { return MapStripeException(ex); }
    }

    private static Result<PaymentGatewayResponse> MapStripeException(StripeException ex)
    {
        var e = ex.StripeError;
        var code = e?.Code ?? "UnknownError";
        var msg = e?.Message ?? ex.Message;
        return Error.BadRequest($"Stripe.{code}", e?.DeclineCode is not null ? $"Stripe [{code}] decline [{e.DeclineCode}]: {msg}" : $"Stripe [{code}]: {msg}");
    }
}
