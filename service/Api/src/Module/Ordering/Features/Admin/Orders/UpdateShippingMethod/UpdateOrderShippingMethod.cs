using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;

namespace Module.Ordering.Features.Admin.Orders.UpdateShippingMethod;
/// <summary>Assigns a shipping method to an order, resets the shipment total, and recalculates all order totals.</summary>
public static partial class UpdateOrderShippingMethod
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Response>
    {
        /// <summary>Assigns the shipping method to an order, resets the shipment total, and recalculates all totals before persisting.</summary>
        /// <param name="command">The command containing the order ID and shipping method ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated order response.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            // Check: Find the order to update the shipping method on.
            var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);
            if (order is null)
                return OrderResult.Errors.NotFound(command.Id);

            // Update: Assign the shipping method, reset shipment total, and recalculate all totals.
            var methodResult = order.SetShippingMethod(command.Request.ShippingMethodId);
            if (methodResult.IsFailure)
                return (Result<Response>)methodResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the updated entity as response.
            return order.MapToDetail<Response>();
        }
    }
}
