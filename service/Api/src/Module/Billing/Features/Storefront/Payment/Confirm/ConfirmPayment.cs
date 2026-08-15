using Module.Billing.Features.Storefront.Payment.Shared.Mappings;

using Module.Ordering.Domain.Orders;

using Module.Billing.Domain.PaymentCaptures;

namespace Module.Billing.Features.Storefront.Payment.Confirm;

/// <summary>Confirms a payment by checking local state — webhook handles async completion.</summary>
public static partial class ConfirmPayment
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Confirms a payment by checking local state.</summary>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Check: Current user must own the order
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return PaymentCaptureResult.Failure.NotFound;

            // Load: Payment capture by ID
            var payment = await dbContext.Set<PaymentCapture>()
                .FirstOrDefaultAsync(p => p.Id == command.Request.PaymentId, cancellationToken);
            if (payment is null)
                return PaymentCaptureResult.Failure.NotFound;

            // Load: Order — verify ownership
            var order = await dbContext.Set<Order>()
                .FirstOrDefaultAsync(o => o.Id == payment.OrderId && o.UserId == userId, cancellationToken);
            if (order is null)
                return PaymentCaptureResult.Failure.NotFound;

            // Check: Already completed by webhook — return immediately
            if (payment.State == PaymentRecordState.Completed)
                return payment.MapToStoreDetail<Response>();

            // Check: State must allow completion
            if (payment.State is not (PaymentRecordState.Processing or PaymentRecordState.Pending))
                return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed);

            // Update: Attempt to complete — webhook may have beaten us
            var completeResult = payment.Complete();
            if (completeResult.IsFailure)
                return payment.MapToStoreDetail<Response>();

            await dbContext.SaveChangesAsync(cancellationToken);

            return payment.MapToStoreDetail<Response>();
        }
    }
}
