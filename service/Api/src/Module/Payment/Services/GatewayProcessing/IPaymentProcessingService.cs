using Module.Payment.Services.Gateways;
using Module.Payment.Domain.PaymentCaptures;

namespace Module.Payment.Services.GatewayProcessing;

public interface IPaymentProcessingService
{
    Task<Result<PaymentProcessingResult>> ProcessAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken ct = default);

    Task<Result<PaymentProcessingResult>> CaptureAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, decimal? amount = null, CancellationToken ct = default);

    Task<Result<PaymentProcessingResult>> VoidAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken ct = default);

    Task<Result<PaymentProcessingResult>> RefundAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, decimal amount, CancellationToken ct = default);

    Task<Result<PaymentProcessingResult>> VoidTransactionAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, object? source = null, CancellationToken ct = default);

    Task<Result<PaymentProcessingResult>> ConfirmAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, CancellationToken ct = default);
}
