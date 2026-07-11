using Module.Payment.Services.Gateways;
using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Services.GatewayProcessing;

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
            var payment = await dbContext.Set<PaymentCapture>()
                .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

            if (payment is null)
                return PaymentCaptureResult.Failure.NotFound;

            var gatewayResult = gatewayRegistry.GetGateway(payment.ProviderKey);
            if (gatewayResult.IsFailure)
                return PaymentCaptureResult.Failure.ProviderNotRegistered(payment.ProviderKey);
            var gateway = gatewayResult.Value;

            var options = new GatewayOptions
            {
                Email = string.Empty,
                Customer = string.Empty,
                OrderId = payment.OrderId.ToString(),
                PaymentId = payment.Number,
                IdempotencyKey = GatewayConstants.Idempotency.ForPayment(payment.Number),
                StatementDescriptorSuffix = string.Empty,
            };

            var voidResult = await processingService.VoidAsync(payment, gateway, options, cancellationToken);
            if (voidResult.IsFailure)
                return voidResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response
            {
                Id = payment.Id,
                Number = payment.Number,
                Amount = payment.Amount,
                State = payment.State,
                Message = voidResult.Message ?? string.Empty
            };
        }
    }
}
