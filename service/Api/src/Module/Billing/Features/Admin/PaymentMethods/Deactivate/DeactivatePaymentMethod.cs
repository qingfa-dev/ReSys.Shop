using Module.Billing.Domain.PaymentMethods;

namespace Module.Billing.Features.Admin.PaymentMethods.Deactivate;

/// <summary>Deactivates a payment method, removing it from storefront availability.</summary>
public static partial class DeactivatePaymentMethod
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command>
    {
        /// <summary>Deactivates the specified payment method and persists the state change.</summary>
        /// <param name="command">The command identifying the payment method to deactivate.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A success result or an error if the payment method is not found or deactivation fails.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=method.Active==false, throws=DbUpdateException
            // Check: Verify the payment method exists.
            var method = await dbContext.Set<PaymentMethod>()
                .FirstOrDefaultAsync(m => m.Id == command.Id, cancellationToken);

            if (method is null)
                return PaymentMethodResult.Errors.NotFound;

            // Update: Deactivate the payment method via domain logic
            var result = method.Deactivate();
            if (result.IsFailure)
                return result;

            // Await: Persist state change
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}