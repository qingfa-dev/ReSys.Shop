using Module.Ordering.Domain.Orders;
using Module.Shipping.Domain.ShippingMethods;

namespace Module.Shipping.Features.Admin.ShippingMethods.Deactivate;

/// <summary>Deactivates a shipping method, preventing new orders from using it.</summary>
public static partial class DeactivateShippingMethod
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command>
    {
        /// <summary>Validates no active orders reference the method, then marks it unavailable.</summary>
        /// <param name="command">The command identifying the shipping method to deactivate.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A success result or an error if not found or has active orders.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=method!=null, post=method.AvailableToUsers==false, throws=DbUpdateException
            // Load: Find the shipping method by ID
            var method = await dbContext.Set<ShippingMethod>()
                .FirstOrDefaultAsync(m => m.Id == command.Id, cancellationToken);

            if (method is null)
                return ShippingMethodResult.Errors.NotFound;

            // Check: Ensure no active orders reference this shipping method
            var hasActiveOrders = await dbContext.Set<Order>()
                .AnyAsync(o => o.ShippingMethodId == command.Id
                    && o.Status != OrderStatus.Canceled
                    && o.Status != OrderStatus.Expired,
                cancellationToken);

            if (hasActiveOrders)
                return ShippingMethodResult.Failure.HasActiveOrders;

            // Update: Mark as unavailable to users
            method.AvailableToUsers = false;

            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}