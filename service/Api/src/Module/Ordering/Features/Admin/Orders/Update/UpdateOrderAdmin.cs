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
                return OrderResult.Errors.NotFound(command.Id);

            var req = command.Request;

            // Update: Apply partial changes using PATCH semantics — only non-null values overwrite.
            var updateResult = order.UpdateDetails(
                req.Email, req.SpecialInstructions,
                req.BillAddressId, req.ShipAddressId, req.ShippingMethodId);
            if (updateResult.IsFailure)
                return (Result<Response>)updateResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<Response>.Ok(order.MapToDetail<Response>(), OrderResult.Success.Updated(order.Id));
        }
    }
}