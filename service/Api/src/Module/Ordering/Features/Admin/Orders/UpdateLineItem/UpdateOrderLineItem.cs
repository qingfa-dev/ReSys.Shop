using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.UpdateLineItem;
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
            // Check: Order exists and is in draft status.
            var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);
            if (order is null)
                return OrderResult.Errors.NotFound(command.OrderId);
            if (order.Status != OrderStatus.Draft)
                return Error.Validation("Order.LineItem.Update.NotDraft", "Only draft orders can have line items modified.");

            // Check: Find the line item.
            var lineItem = await dbContext.Set<LineItem>().FirstOrDefaultAsync(li => li.Id == command.LineItemId && li.OrderId == command.OrderId, cancellationToken);
            if (lineItem is null)
                return LineItemResult.Errors.NotFound(command.LineItemId);

            // Update: Apply quantity change.
            var updateResult = lineItem.UpdateQuantity(command.Request.Quantity);
            if (updateResult.IsFailure)
                return updateResult.Errors;

            // Update: Recalculate order totals.
            order.RecalculateTotals();

            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the updated entity as response.
            return new Response { Id = lineItem.Id, Quantity = lineItem.Quantity, Total = lineItem.Total };
        }
    }
}
