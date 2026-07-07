using Module.Payment.Domain.PaymentMethods;

namespace Module.Payment.Features.Admin.PaymentMethods.Delete;

    /// <summary>Handles DeletePaymentMethod feature.</summary>
    public static partial class DeletePaymentMethod
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {

        // Contract: pre=command!=null, post=result!=null
            // Check: Verify the payment method exists.
            var method = await dbContext.Set<PaymentMethod>()
                .FirstOrDefaultAsync(m => m.Id == command.Id, cancellationToken);

            if (method is null)
                return PaymentMethodResult.Errors.NotFound;

            // Soft Delete: Mark the payment method as deleted.
            method.IsDeleted = true;
            method.DeletedAtUtc = DateTimeOffset.UtcNow;
            method.DeletedBy = "System";
            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
