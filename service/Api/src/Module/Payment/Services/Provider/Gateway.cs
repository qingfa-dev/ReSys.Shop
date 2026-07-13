namespace Module.Payment.Services.Provider;

// Contract: pre=amount>0 && options!=null, post=Result<PaymentGatewayResponse>
// Invariant: AutoCapture==true for Stripe, false for manual gateways
public abstract class Gateway : IPaymentGatewayActionProvider
{
    protected const decimal FromDollarToCentRate = 100m;

    public abstract string ProviderKey { get; }
    public abstract bool AutoCapture { get; }
    public abstract bool SourceRequired { get; }
    public abstract bool PaymentProfilesSupported { get; }
    public abstract bool Supports(object? source);

    public abstract Task<Result<PaymentGatewayResponse>> AuthorizeAsync(
        decimal amount, object? source, GatewayOptions options, CancellationToken ct = default);

    public abstract Task<Result<PaymentGatewayResponse>> CaptureAsync(
        decimal amount, string? responseCode, GatewayOptions options, CancellationToken ct = default);

    public abstract Task<Result<PaymentGatewayResponse>> PurchaseAsync(
        decimal amount, object? source, GatewayOptions options, CancellationToken ct = default);

    public abstract Task<Result<PaymentGatewayResponse>> VoidAsync(
        string? responseCode, object? source, GatewayOptions options, CancellationToken ct = default);

    public abstract Task<Result<PaymentGatewayResponse>> RefundAsync(
        decimal amount, string? responseCode, GatewayOptions options, CancellationToken ct = default);

    public abstract Task<Result<PaymentGatewayResponse>> CreateSetupIntentAsync(
        string? customerId, Dictionary<string, string>? metadata, CancellationToken ct = default);

    // Call: Default succeeds — subclasses may override for real status check
    public virtual Task<string> GetPaymentStatusAsync(
        string responseCode, CancellationToken ct = default)
        => Task.FromResult("succeeded");

    public virtual decimal ExchangeMultiplier => FromDollarToCentRate;
    public virtual string? GatewayDashboardPaymentUrl(object? payment) => null;
    public virtual Dictionary<string, string?> Options => [];
    public virtual string[] Actions => ["authorize", "capture", "purchase", "void", "refund"];
}