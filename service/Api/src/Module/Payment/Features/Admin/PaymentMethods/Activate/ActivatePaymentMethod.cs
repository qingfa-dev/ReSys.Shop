using Module.Payment.Domain.PaymentMethods;

namespace Module.Payment.Features.Admin.PaymentMethods.Activate;

/// <summary>Activates a payment method, making it available for storefront use.</summary>
public static partial class ActivatePaymentMethod
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command>
    {
        /// <summary>Activates the specified payment method and persists the state change.</summary>
        /// <param name="command">The command identifying the payment method to activate.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A success result or an error if the payment method is not found or activation fails.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=method.Active==true, throws=DbUpdateException
            // Check: Verify the payment method exists.
            var method = await dbContext.Set<PaymentMethod>()
                .FirstOrDefaultAsync(m => m.Id == command.Id, cancellationToken);

            if (method is null)
                return PaymentMethodResult.Errors.NotFound;

            // Update: Activate the payment method via domain logic
            var result = method.Activate();
            if (result.IsFailure)
                return result;

            // Await: Persist state change
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}