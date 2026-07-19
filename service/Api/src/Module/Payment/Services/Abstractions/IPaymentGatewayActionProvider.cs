using Module.Payment.Services.Provider;

// Context: Legacy duplicate of Services.Provider.IPaymentGatewayActionProvider
namespace Module.Payment.Services.Abstractions;

public interface IPaymentGatewayActionProvider
{
    string ProviderKey { get; }
    bool AutoCapture { get; }
    bool SourceRequired { get; }
    bool PaymentProfilesSupported { get; }
    bool Supports(object? source);

    Task<Result<PaymentGatewayResponse>> AuthorizeAsync(
        decimal amount, object? source, GatewayOptions options, CancellationToken ct = default);

    Task<Result<PaymentGatewayResponse>> CaptureAsync(
        decimal amount, string? responseCode, GatewayOptions options, CancellationToken ct = default);

    Task<Result<PaymentGatewayResponse>> PurchaseAsync(
        decimal amount, object? source, GatewayOptions options, CancellationToken ct = default);

    Task<Result<PaymentGatewayResponse>> VoidAsync(
        string? responseCode, object? source, GatewayOptions options, CancellationToken ct = default);

    Task<Result<PaymentGatewayResponse>> RefundAsync(
        decimal amount, string? responseCode, GatewayOptions options, CancellationToken ct = default);

    Task<Result<PaymentGatewayResponse>> CreateSetupIntentAsync(
        string? customerId, Dictionary<string, string>? metadata, CancellationToken ct = default);

    Task<string> GetPaymentStatusAsync(
        string responseCode, CancellationToken ct = default);
}