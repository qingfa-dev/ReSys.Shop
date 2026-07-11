using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.PaymentCaptures;

namespace Module.Payment.Features.Storefront.Payment.Confirm;

public static partial class ConfirmPayment
{
    public sealed record Command(Guid PaymentId) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser, IPaymentGatewayActionProvider gateway)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return PaymentCaptureResult.Failure.NotFound;

            var payment = await dbContext.Set<PaymentCapture>()
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == command.PaymentId && p.Order.UserId == userId, cancellationToken);

            // Check: Verify the payment exists.
            if (payment is null)
                return PaymentCaptureResult.Failure.NotFound;

            // Validate: Payment must be in Processing or Pending state to confirm
            if (payment.State is not (PaymentRecordState.Processing or PaymentRecordState.Pending))
            {
                // Validate: Check business rule.
                if (payment.State is PaymentRecordState.Completed)
                    return PaymentCaptureResult.Failure.AlreadyCompleted;

                return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed);
            }

            // Verify: Check payment intent status at the gateway
            var status = await gateway.GetPaymentIntentStatusAsync(payment.ResponseCode!, cancellationToken);
            if (status != "succeeded")
                return PaymentCaptureResult.Failure.NotSucceeded;

            // Transition: Complete the payment
            var completeResult = payment.Complete();
            if (completeResult.IsFailure)
                return completeResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the result.
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
