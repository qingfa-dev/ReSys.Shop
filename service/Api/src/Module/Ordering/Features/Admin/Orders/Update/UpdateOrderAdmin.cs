using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;

namespace Module.Ordering.Features.Admin.Orders.Update;
/// <summary>Updates editable fields on a draft order using patch semantics — only non-null request values overwrite the existing order properties.</summary>
public static partial class UpdateOrderAdmin
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Response>
    {
        /// <summary>Applies partial updates (patch semantics) to a draft order and persists changes.</summary>
        /// <param name="command">The command containing the order ID and updated fields.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated order response.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            // Check: Find the order to update.
            var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);
            if (order is null)
                return OrderResult.Failure.NotFound(command.Id);

            // Enforce: Only draft orders can be modified — prevents edits to placed orders.
            if (order.Status != OrderStatus.Draft)
                return Error.Validation("Order.Update.NotDraft", "Only draft orders can be modified.");

            var req = command.Request;

            // Update: Apply partial changes using PATCH semantics — only non-null values overwrite.
            if (req.Email is not null) order.Email = req.Email;
            if (req.SpecialInstructions is not null) order.SpecialInstructions = req.SpecialInstructions;
            if (req.BillAddressId.HasValue) order.BillAddressId = req.BillAddressId;
            if (req.ShipAddressId.HasValue) order.ShipAddressId = req.ShipAddressId;
            if (req.ShippingMethodId.HasValue) order.ShippingMethodId = req.ShippingMethodId;
            order.ModifiedAtUtc = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the updated entity as response.
            return order.MapToDetail<Response>();
        }
    }
}
