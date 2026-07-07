using Microsoft.EntityFrameworkCore;
using Module.Payment.Domain.Payments;
using PaymentDomain = Module.Payment.Domain.Payments.Payment;

namespace Module.Payment.Features.Storefront.Payment.Confirm;

    /// <summary>Handles ConfirmPayment feature.</summary>
    public static partial class ConfirmPayment
{
    public sealed record Command(Guid PaymentId) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {

        // Contract: pre=command!=null, post=result!=null
            // Query: Get payment by ID
            var payment = await dbContext.Set<PaymentDomain>()
                .FirstOrDefaultAsync(p => p.Id == command.PaymentId, cancellationToken);

            // Check: Verify the payment exists.
            if (payment is null)
                return PaymentResult.Errors.NotFound;

            // Validate: Payment must be in Processing or Pending state to confirm
            if (payment.State is not (PaymentState.Processing or PaymentState.Pending))
            {
                // Validate: Check business rule.
                if (payment.State is PaymentState.Completed)
                    return PaymentResult.Errors.AlreadyCompleted;

                return PaymentResult.Errors.InvalidStateTransition(payment.State, PaymentState.Completed);
            }

            // Transition: Complete the payment
            var completeResult = payment.Complete();
            if (completeResult.IsFailure)
                return completeResult.Failures;

            // Persist: Save changes to the database.
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
