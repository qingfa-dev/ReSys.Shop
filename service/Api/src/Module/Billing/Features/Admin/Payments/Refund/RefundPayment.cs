using Module.Billing.Features.Admin.Shared.Mappings;

using GatewayOptions = Module.Billing.Services.Provider.GatewayOptions;
using IGatewayRegistry = Module.Billing.Services.Provider.IGatewayRegistry;
using IPaymentProcessingService = Module.Billing.Services.Processing.IPaymentProcessingService;

using Module.Billing.Services.Provider;
using Module.Billing.Domain.PaymentCaptures;



namespace Module.Billing.Features.Admin.Payments.Refund;

/// <summary>Refunds a completed payment.</summary>
public static partial class RefundPayment
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, IGatewayRegistry gatewayRegistry, IPaymentProcessingService processingService)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Refunds a completed payment.</summary>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Load: Payment capture by ID
            var payment = await dbContext.Set<PaymentCapture>()
                .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

            // Check: Payment must exist
            if (payment is null)
                return PaymentCaptureResult.Failure.NotFound;

            // Check: Gateway must be registered for the payment's provider
            var gatewayResult = gatewayRegistry.GetGateway(payment.ProviderKey);
            if (gatewayResult.IsFailure)
                return PaymentCaptureResult.Failure.ProviderNotRegistered(payment.ProviderKey);
            var gateway = gatewayResult.Value;

            // Build: Gateway options with idempotency key
            var options = new GatewayOptions
            {
                Email = string.Empty,
                Customer = string.Empty,
                OrderId = payment.OrderId.ToString(),
                PaymentId = payment.Number,
                IdempotencyKey = GatewayConstants.Idempotency.ForPayment(payment.Number),
                StatementDescriptorSuffix = string.Empty,
            };

            var refundAmount = command.Request.Amount;

            // Call: Gateway refund — PaymentProcessingService delegates to Stripe Refund
            var refundResult = await processingService.RefundAsync(payment, gateway, options, refundAmount, cancellationToken);
            if (refundResult.IsFailure)
                return refundResult.Errors;

            // Persist: Retry on concurrency conflict with webhook (max 3 attempts)
            for (int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                    break;
                }
                catch (DbUpdateConcurrencyException) when (attempt < 2)
                {
                    // Reload: Re-query payment from DB and re-validate refund eligibility
                    payment = await dbContext.Set<PaymentCapture>()
                        .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);
                    if (payment is null)
                        return PaymentCaptureResult.Failure.NotFound;
                    if (!payment.CanRefund(refundAmount))
                        return PaymentCaptureResult.Failure.AmountExceedsAuthorized;
                    // Retry: Gateway already called, only need to persist domain state
                    var retryResult = payment.Refund(refundAmount);
                    if (retryResult.IsFailure)
                        return Result<Response>.Failure(retryResult.Errors[0]);
                }
            }

            // Map: Payment → response DTO
            var response = payment.MapToDetail<Response>();
            response.RefundedAmount = refundAmount;
            response.Message = refundResult.Message ?? string.Empty;
            return response;
        }
    }
}
