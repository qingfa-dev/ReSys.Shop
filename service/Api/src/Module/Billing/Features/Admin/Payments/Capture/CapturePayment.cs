using Module.Billing.Features.Admin.Shared.Mappings;

using GatewayOptions = Module.Billing.Services.Provider.GatewayOptions;
using IGatewayRegistry = Module.Billing.Services.Provider.IGatewayRegistry;
using IPaymentProcessingService = Module.Billing.Services.Processing.IPaymentProcessingService;

using Module.Billing.Services.Provider;
using Module.Billing.Domain.PaymentCaptures;
using Module.Ordering.Features.Storefront.RecordOrderPaymentState;



namespace Module.Billing.Features.Admin.Payments.Capture;

// Contract: pre=command.Id valid, post=payment.CapturedAmount set || Result.IsFailure
/// <summary>Captures an authorized payment.</summary>
public static partial class CapturePayment
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        IGatewayRegistry gatewayRegistry,
        IPaymentProcessingService processingService,
        ISender sender,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Captures an authorized payment.</summary>
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

            // Compute: Capture amount — default to remaining uncaptured amount
            var captureAmount = command.Request.Amount ?? payment.UncapturedAmount();

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

            // Call: Gateway capture — PaymentProcessingService delegates to Stripe Capture
            var captureResult = await processingService.CaptureAsync(payment, gateway, options, captureAmount, cancellationToken);
            if (captureResult.IsFailure)
                return captureResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Mirror: capture completes the payment → stamp the order's payment timeline.
            // Best-effort — the payment row is authoritative.
            // TODO(audit 2026-08-16): cross-module ISender — RecordOrderPaymentStateCommand is just
            // order.MarkPaymentCompleted(...); load Order and call the domain method directly.
            var notifyResult = await sender.Send(new RecordOrderPaymentStateCommand
            {
                OrderId = payment.OrderId,
                PaymentState = PaymentTimelineState.Completed,
                AtUtc = payment.CompletedAtUtc ?? DateTimeOffset.UtcNow
            }, cancellationToken);
            if (notifyResult.IsFailure)
                logger.LogWarning("Failed to mirror payment completion onto order for payment {PaymentId}: {Message}", payment.Id, notifyResult.Message);

            // Map: Payment → response DTO
            var response = payment.MapToDetail<Response>();
            response.CapturedAmount = captureAmount;
            response.Message = captureResult.Message ?? string.Empty;
            return response;
        }
    }
}
