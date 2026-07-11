using Module.Payment.Domain.PaymentCaptures;

namespace Module.Payment.Domain.Gateways;

/// <summary>Base gateway class ported from Spree::Gateway — delegates payment operations to the provider.</summary>
// Invariant: ExchangeMultiplier converts dollars to cents (100.0); Provider is lazy-initialized from Preferences
// Define: Base gateway class ported from Spree::Gateway -- delegates active* operations to the provider
public abstract partial class Gateway : IPaymentGatewayActionProvider
{
    // Constant: Exchange rate multiplier for converting dollars to cents -- mirrors Ruby FROM_DOLLAR_TO_CENT_RATE
    protected const decimal FromDollarToCentRate = 100m;

    public abstract bool AutoCapture { get; }
    public abstract bool SourceRequired { get; }
    public abstract bool PaymentProfilesSupported { get; }
    public abstract bool Supports(object? source);

    // Call: Authorize delegates to provider.authorize -- mirrors Ruby delegate :authorize, to: :provider
    public abstract Task<Result<PaymentGatewayResponse>> AuthorizeAsync(decimal amountInCents, object? source, GatewayOptions options, CancellationToken cancellationToken = default);

    // Call: Capture delegates to provider.capture -- mirrors Ruby delegate :capture, to: :provider
    public abstract Task<Result<PaymentGatewayResponse>> CaptureAsync(decimal amount, string? responseCode, GatewayOptions options, CancellationToken cancellationToken = default);

    // Call: Purchase delegates to provider.purchase -- mirrors Ruby delegate :purchase, to: :provider
    public abstract Task<Result<PaymentGatewayResponse>> PurchaseAsync(decimal amountInCents, object? source, GatewayOptions options, CancellationToken cancellationToken = default);

    // Call: Void delegates to provider.void -- mirrors Ruby delegate :void, to: :provider
    public abstract Task<Result<PaymentGatewayResponse>> VoidAsync(string? responseCode, object? source, GatewayOptions options, CancellationToken cancellationToken = default);

    // Call: Cancel delegates to provider.cancel -- mirrors Ruby cancel(response_code, payment)
    public abstract Task<Result<PaymentGatewayResponse>> CancelAsync(string? responseCode, object? payment, CancellationToken cancellationToken = default);

    // Call: Credit delegates to provider.credit -- mirrors Ruby delegate :credit, to: :provider
    public abstract Task<Result<PaymentGatewayResponse>> CreditAsync(decimal amount, string? responseCode, GatewayOptions options, CancellationToken cancellationToken = default);

    // Compute: Exchange multiplier for converting store amounts to gateway amounts -- mirrors Ruby exchange_multiplier
    public virtual decimal ExchangeMultiplier => FromDollarToCentRate;

    // Compute: Gateway dashboard URL for a given payment -- mirrors Ruby gateway_dashboard_payment_url(payment)
    public virtual string? GatewayDashboardPaymentUrl(object? payment) => null;

    // Compute: Active merchant preferences hash -- mirrors Ruby options (preferences as symbol-keyed hash)
    public virtual Dictionary<string, string?> Options => [];

    // Compute: List of actions this gateway supports -- mirrors Ruby actions method
    public virtual string[] Actions => ["authorize", "capture", "purchase", "void", "credit"];

    // Call: Retrieve payment intent status from gateway
    public virtual Task<string> GetPaymentIntentStatusAsync(string paymentIntentId, CancellationToken cancellationToken = default)
        => Task.FromResult("succeeded");
}