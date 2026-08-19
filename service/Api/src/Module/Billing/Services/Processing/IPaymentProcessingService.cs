using Module.Billing.Services.Provider;

using Module.Billing.Domain.PaymentCaptures;

namespace Module.Billing.Services.Processing;

/// <summary>Defines payment gateway operations — process, capture, void, refund — with state management.</summary>
public interface IPaymentProcessingService
{
    /// <summary>Routes payment to purchase or authorize based on gateway auto-capture setting.</summary>
    Task<Result<PaymentProcessingResult>> ProcessAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken ct = default);

    /// <summary>Captures an authorized payment, transitioning state to Completed.</summary>
    Task<Result<PaymentProcessingResult>> CaptureAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, decimal? amount = null, CancellationToken ct = default);

    /// <summary>Voids a payment via the gateway.</summary>
    Task<Result<PaymentProcessingResult>> VoidAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken ct = default);

    /// <summary>Refunds a completed payment.</summary>
    Task<Result<PaymentProcessingResult>> RefundAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, decimal amount, CancellationToken ct = default);

    /// <summary>Voids a transaction with optional source profile support.</summary>
    Task<Result<PaymentProcessingResult>> VoidTransactionAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, object? source = null, CancellationToken ct = default);

    /// <summary>Confirms a payment, transitioning to Completed or Pending based on gateway.</summary>
    Task<Result<PaymentProcessingResult>> ConfirmAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, CancellationToken ct = default);
}