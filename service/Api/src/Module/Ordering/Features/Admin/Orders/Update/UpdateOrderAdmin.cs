using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;

namespace Module.Ordering.Features.Admin.Orders.Update;
/// <summary>Handles UpdateOrderAdmin feature.</summary>
public static partial class UpdateOrderAdmin
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Response>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Check: Verify the order exists.
            var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);
            if (order is null)
                return OrderResult.Errors.NotFound(command.Id);

            var req = command.Request;

            // Update: Apply partial changes (PATCH semantics).
            if (req.Email is not null) order.Email = req.Email;
            if (req.SpecialInstructions is not null) order.SpecialInstructions = req.SpecialInstructions;
            if (req.BillAddressId.HasValue) order.BillAddressId = req.BillAddressId;
            if (req.ShipAddressId.HasValue) order.ShipAddressId = req.ShipAddressId;
            if (req.ShippingMethodId.HasValue) order.ShippingMethodId = req.ShippingMethodId;
            order.ModifiedAtUtc = DateTimeOffset.UtcNow;

            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the updated entity as response.
            return order.MapToDetail<Response>();
        }
    }
}
