using Module.Ordering.Domain.Orders;
using Module.Shipping.Domain.ShippingMethods;

namespace Module.Shipping.Features.Admin.ShippingMethods.Deactivate;

public static partial class DeactivateShippingMethod
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var method = await dbContext.Set<ShippingMethod>()
                .FirstOrDefaultAsync(m => m.Id == command.Id, cancellationToken);

            if (method is null)
                return ShippingMethodResult.Errors.NotFound;

            var hasActiveOrders = await dbContext.Set<Order>()
                .AnyAsync(o => o.ShippingMethodId == command.Id
                    && o.Status != OrderStatus.Canceled
                    && o.Status != OrderStatus.Expired,
                cancellationToken);

            if (hasActiveOrders)
                return ShippingMethodResult.Failure.HasActiveOrders;

            method.AvailableToUsers = false;

            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}
