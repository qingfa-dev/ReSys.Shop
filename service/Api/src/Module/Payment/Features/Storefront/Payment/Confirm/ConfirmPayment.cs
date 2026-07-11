using Module.Payment.Services.Abstractions;
using Module.Payment.Services.Models;
using Module.Payment.Services.Gateways;
using Module.Payment.Domain.PaymentCaptures;

namespace Module.Payment.Features.Storefront.Payment.Confirm;

public static partial class ConfirmPayment
{
    public sealed record Command(Guid PaymentId) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IGatewayRegistry gatewayRegistry)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return PaymentCaptureResult.Failure.NotFound;

            var payment = await dbContext.Set<PaymentCapture>()
                .FirstOrDefaultAsync(p => p.Id == command.PaymentId, cancellationToken);
            if (payment is null)
                return PaymentCaptureResult.Failure.NotFound;

            if (payment.State is not (PaymentRecordState.Processing or PaymentRecordState.Pending))
            {
                if (payment.State is PaymentRecordState.Completed)
                    return PaymentCaptureResult.Failure.AlreadyCompleted;
                return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed);
            }

            var gatewayResult = gatewayRegistry.GetGateway(payment.ProviderKey);
            if (gatewayResult.IsFailure)
                return PaymentCaptureResult.Failure.ProviderNotRegistered(payment.ProviderKey);
            var gateway = gatewayResult.Value;

            if (string.IsNullOrEmpty(payment.ResponseCode))
                return PaymentCaptureResult.Failure.NotSucceeded;

            var status = await gateway.GetPaymentStatusAsync(payment.ResponseCode, cancellationToken);
            if (status != GatewayConstants.Stripe.IntentStatus.Succeeded)
                return PaymentCaptureResult.Failure.NotSucceeded;

            var completeResult = payment.Complete();
            if (completeResult.IsFailure) return completeResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response
            {
                Id = payment.Id,
                Number = payment.Number,
                Amount = payment.Amount,
                State = payment.State,
                Message = completeResult.Message ?? "Payment confirmed."
            };
        }
    }
}
