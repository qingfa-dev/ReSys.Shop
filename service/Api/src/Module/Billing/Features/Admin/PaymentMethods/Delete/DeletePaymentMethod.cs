using Module.Billing.Domain.PaymentMethods;
using Module.Billing.Domain.PaymentCaptures;

namespace Module.Billing.Features.Admin.PaymentMethods.Delete;

/// <summary>Soft-deletes a payment method, preventing new usage while preserving referential integrity.</summary>
public static partial class DeletePaymentMethod
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command>
    {
        /// <summary>Validates no active payments reference the method, then performs soft-delete.</summary>
        /// <param name="command">The command identifying the payment method to delete.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A success result or an error if the method is not found or has active payments.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=method.IsDeleted==true, throws=DbUpdateException
            // Check: Verify the payment method exists.
            var method = await dbContext.Set<PaymentMethod>()
                .FirstOrDefaultAsync(m => m.Id == command.Id, cancellationToken);

            if (method is null)
                return PaymentMethodResult.Errors.NotFound;

            // Check: Verify no active payments reference this payment method.
            var hasActivePayments = await dbContext.Set<PaymentCapture>()
                .AnyAsync(p => p.PaymentMethodId == command.Id
                    && p.State != PaymentRecordState.Completed
                    && p.State != PaymentRecordState.Failed
                    && p.State != PaymentRecordState.Void
                    && p.State != PaymentRecordState.Invalid,
                cancellationToken);

            if (hasActivePayments)
                return PaymentMethodResult.Errors.HasActivePayments;

            // Remove: Soft-delete the payment method.
            method.IsDeleted = true;
            method.DeletedAtUtc = DateTimeOffset.UtcNow;
            method.DeletedBy = "System";
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}