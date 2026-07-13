using Module.Payment.Features.Admin.Payments.Shared.Mappings;

using GatewayOptions = Module.Payment.Services.Provider.GatewayOptions;
using IGatewayRegistry = Module.Payment.Services.Provider.IGatewayRegistry;
using IPaymentProcessingService = Module.Payment.Services.Processing.IPaymentProcessingService;

using Module.Payment.Services.Models;
using Module.Payment.Domain.PaymentCaptures;

using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.Payment.Features.Admin.Payments.Void;

public static partial class VoidPayment
{
    public sealed record Command(Guid Id) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, IGatewayRegistry gatewayRegistry, IPaymentProcessingService processingService)
        : ICommandHandler<Command, Response>
    {
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

            // Call: Gateway void — PaymentProcessingService delegates to Stripe Cancel
            var voidResult = await processingService.VoidAsync(payment, gateway, options, cancellationToken);
            if (voidResult.IsFailure)
                return voidResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Payment → response DTO
            var response = payment.MapToDetail<Response>();
            response.Message = voidResult.Message ?? string.Empty;
            return response;
        }
    }
}
