using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Shared.Extensions;
using Module.Ordering.Features.Admin.Shared.Mappings;

namespace Module.Ordering.Features.Admin.Orders.LineItems.UpdateLineItem;
/// <summary>Updates the quantity of an existing line item on a draft order and recalculates order totals.</summary>
public static partial class UpdateOrderLineItem
{
    public sealed record Command(Guid OrderId, Guid LineItemId, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Response>
    {
        /// <summary>Applies a quantity change to a line item via domain logic and recalculates the parent order's totals.</summary>
        /// <param name="command">The command containing the order ID, line item ID, and new quantity.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated line item response.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            // Check: Find the parent order for status validation.
            var order = await dbContext.Set<Order>().IncludeOrderDetail().FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);
            if (order is null)
                return OrderResult.Errors.NotFound(command.OrderId);
            // Enforce: Only draft orders can have line items modified.
            if (!order.CanModifyLineItems())
                return OrderResult.Errors.NotDraftForLineItem;

            // Check: Find the line item scoped to its parent order.
            var lineItem = await dbContext.Set<LineItem>().FirstOrDefaultAsync(li => li.Id == command.LineItemId && li.OrderId == command.OrderId, cancellationToken);
            if (lineItem is null)
                return LineItemResult.Errors.NotFound(command.LineItemId);

            // Update: Apply quantity change through domain logic.
            var updateResult = lineItem.UpdateQuantity(command.Request.Quantity);
            if (updateResult.IsFailure)
                return updateResult.Errors;

            // Update: Recalculate order totals after line item quantity change.
            var recalcResult = order.RecalculateTotals();
            if (recalcResult.IsFailure)
                return recalcResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the updated order with full detail.
            return Result<Response>.Ok(order.MapToDetail<Response>(), LineItemResult.Success.Updated(command.LineItemId));
        }
    }
}