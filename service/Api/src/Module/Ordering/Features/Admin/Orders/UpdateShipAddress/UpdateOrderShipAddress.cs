using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;

namespace Module.Ordering.Features.Admin.Orders.UpdateShipAddress;
/// <summary>Updates the shipping address on a draft order, enforcing draft-only status before persisting the change.</summary>
public static partial class UpdateOrderShipAddress
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Response>
    {
        /// <summary>Sets the shipping address on a draft order after validating the order exists and is still editable.</summary>
        /// <param name="command">The command containing the order ID and address ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated order response.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            // Check: Order exists and is in draft status.
            var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);
            if (order is null)
                return OrderResult.Errors.NotFound(command.Id);

            if (order.Status != OrderStatus.Draft)
                return Error.Validation("Order.ShipAddress.Update.NotDraft", "Only draft orders can have shipping address modified.");

            // Update: Set the shipping address.
            order.ShipAddressId = command.Request.AddressId;
            order.ModifiedAtUtc = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the updated entity as response.
            return order.MapToDetail<Response>();
        }
    }
}
