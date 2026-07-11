using Module.Payment.Domain.PaymentCaptures;

namespace Module.Payment.Domain.Gateways;

/// <summary>Defines the contract for payment gateway actions — domain abstractions with concrete providers in Infrastructure/Gateways/.</summary>
// Boundary: Domain -- Infrastructure -- gateways are domain abstractions; concrete providers live in Infrastructure/Gateways/
// Contract: pre=gateway!=null, post=response.IsSuccess==true|false, returns Task<Result<PaymentGatewayResponse>>
public partial interface IPaymentGatewayActionProvider
{
    // Compute: Whether payments should auto-capture (purchase) instead of authorize-only -- mirrors Ruby auto_capture?
    bool AutoCapture { get; }

    // Compute: Whether a payment source (e.g. credit card) is required -- mirrors Ruby source_required?
    bool SourceRequired { get; }

    // Compute: Whether payment profiles (stored credentials) are supported -- mirrors Ruby payment_profiles_supported?
    bool PaymentProfilesSupported { get; }

    // Compute: Whether the gateway supports a given payment source -- mirrors Ruby supports?(source)
    bool Supports(object? source);

    // Call: Gateway authorize -- reserves funds on the payment source -- mirrors Ruby authorize(amount, source, gateway_options)
    Task<Result<PaymentGatewayResponse>> AuthorizeAsync(decimal amountInCents, object? source, GatewayOptions options, CancellationToken cancellationToken = default);

    // Call: Gateway capture -- settles previously authorized funds -- mirrors Ruby capture(amount, response_code, gateway_options)
    Task<Result<PaymentGatewayResponse>> CaptureAsync(decimal amount, string? responseCode, GatewayOptions options, CancellationToken cancellationToken = default);

    // Call: Gateway purchase -- combines authorize+capture in one step -- mirrors Ruby purchase(amount, source, gateway_options)
    Task<Result<PaymentGatewayResponse>> PurchaseAsync(decimal amountInCents, object? source, GatewayOptions options, CancellationToken cancellationToken = default);

    // Call: Gateway void -- cancels an authorization before settlement -- mirrors Ruby void(response_code, source, gateway_options)
    Task<Result<PaymentGatewayResponse>> VoidAsync(string? responseCode, object? source, GatewayOptions options, CancellationToken cancellationToken = default);

    // Call: Gateway cancel -- voids a completed/captured payment -- mirrors Ruby cancel(response_code, payment)
    Task<Result<PaymentGatewayResponse>> CancelAsync(string? responseCode, object? payment, CancellationToken cancellationToken = default);

    // Call: Gateway credit -- issues a refund/credit against a captured payment -- mirrors Ruby credit(amount, response_code, gateway_options)
    Task<Result<PaymentGatewayResponse>> CreditAsync(decimal amount, string? responseCode, GatewayOptions options, CancellationToken cancellationToken = default);

    // Call: Retrieve payment intent status from gateway
    Task<string> GetPaymentIntentStatusAsync(string paymentIntentId, CancellationToken cancellationToken = default);
}