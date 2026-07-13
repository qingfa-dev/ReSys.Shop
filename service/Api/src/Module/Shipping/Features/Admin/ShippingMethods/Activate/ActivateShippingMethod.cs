using Module.Shipping.Domain.ShippingMethods;

namespace Module.Shipping.Features.Admin.ShippingMethods.Activate;

/// <summary>Activates a shipping method, making it available to storefront users.</summary>
public static partial class ActivateShippingMethod
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command>
    {
        /// <summary>Sets the shipping method as available to users and persists the change.</summary>
        /// <param name="command">The command identifying the shipping method to activate.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A success result or a not-found error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=method!=null, post=method.AvailableToUsers==true, throws=DbUpdateException
            // Load: Find the shipping method by ID
            var method = await dbContext.Set<ShippingMethod>()
                .FirstOrDefaultAsync(m => m.Id == command.Id, cancellationToken);

            if (method is null)
                return ShippingMethodResult.Errors.NotFound;

            // Update: Mark as available to users
            method.AvailableToUsers = true;

            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}