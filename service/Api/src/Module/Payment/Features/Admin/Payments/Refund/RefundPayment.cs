using Module.Payment.Services.Abstractions;
using Module.Payment.Services.Models;
using Module.Payment.Domain.PaymentCaptures;

using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.Payment.Features.Admin.Payments.Refund;

public static partial class RefundPayment
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

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

            var refundAmount = command.Request.Amount;

            var refundResult = await processingService.RefundAsync(payment, gateway, options, refundAmount, cancellationToken);
            if (refundResult.IsFailure)
                return refundResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response
            {
                Id = payment.Id,
                Number = payment.Number,
                Amount = command.Request.Amount,
                RefundedAmount = refundAmount,
                State = payment.State,
                Message = refundResult.Message ?? string.Empty
            };
        }
    }
}
