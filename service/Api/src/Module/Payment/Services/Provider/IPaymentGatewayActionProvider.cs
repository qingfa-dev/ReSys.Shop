namespace Module.Payment.Services.Provider;

/// <summary>Defines the contract for a payment gateway action provider.</summary>
public interface IPaymentGatewayActionProvider
{
    /// <summary>The unique provider key.</summary>
    string ProviderKey { get; }

    /// <summary>Whether the gateway auto-captures payments.</summary>
    bool AutoCapture { get; }

    /// <summary>Whether a payment source is required.</summary>
    bool SourceRequired { get; }

    /// <summary>Whether payment profiles are supported.</summary>
    bool PaymentProfilesSupported { get; }

    /// <summary>Checks whether the given source is supported.</summary>
    /// <param name="source">The source object to check.</param>
    /// <returns>True if supported.</returns>
    bool Supports(object? source);

    /// <summary>Authorizes a payment amount.</summary>
    /// <param name="amount">The amount to authorize.</param>
    /// <param name="source">The payment source.</param>
    /// <param name="options">Gateway-specific options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The gateway response.</returns>
    Task<Result<PaymentGatewayResponse>> AuthorizeAsync(
        decimal amount, object? source, GatewayOptions options, CancellationToken ct = default);

    /// <summary>Captures an authorized payment.</summary>
    /// <param name="amount">The amount to capture.</param>
    /// <param name="responseCode">The authorization response code.</param>
    /// <param name="options">Gateway-specific options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The gateway response.</returns>
    Task<Result<PaymentGatewayResponse>> CaptureAsync(
        decimal amount, string? responseCode, GatewayOptions options, CancellationToken ct = default);

    /// <summary>Processes a purchase (authorize + capture in one step).</summary>
    /// <param name="amount">The purchase amount.</param>
    /// <param name="source">The payment source.</param>
    /// <param name="options">Gateway-specific options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The gateway response.</returns>
    Task<Result<PaymentGatewayResponse>> PurchaseAsync(
        decimal amount, object? source, GatewayOptions options, CancellationToken ct = default);

    /// <summary>Voids a transaction.</summary>
    /// <param name="responseCode">The transaction response code.</param>
    /// <param name="source">Optional source override.</param>
    /// <param name="options">Gateway-specific options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The gateway response.</returns>
    Task<Result<PaymentGatewayResponse>> VoidAsync(
        string? responseCode, object? source, GatewayOptions options, CancellationToken ct = default);

    /// <summary>Refunds a captured payment.</summary>
    /// <param name="amount">The amount to refund.</param>
    /// <param name="responseCode">The capture response code.</param>
    /// <param name="options">Gateway-specific options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The gateway response.</returns>
    Task<Result<PaymentGatewayResponse>> RefundAsync(
        decimal amount, string? responseCode, GatewayOptions options, CancellationToken ct = default);

    /// <summary>Creates a setup intent for saving payment methods.</summary>
    /// <param name="customerId">Optional customer ID.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The gateway response.</returns>
    Task<Result<PaymentGatewayResponse>> CreateSetupIntentAsync(
        string? customerId, Dictionary<string, string>? metadata, CancellationToken ct = default);

    /// <summary>Gets the payment status from a response code.</summary>
    /// <param name="responseCode">The gateway response code.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The payment status string.</returns>
    Task<string> GetPaymentStatusAsync(
        string responseCode, CancellationToken ct = default);
}