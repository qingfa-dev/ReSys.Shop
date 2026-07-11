using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.RemoveLineItem;
/// <summary>Removes a line item from a draft order and recalculates the order totals.</summary>
public static partial class RemoveOrderLineItem
{
    public sealed record Command(Guid OrderId, Guid LineItemId) : ICommand;
    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command>
    {
        /// <summary>Validates the order is in draft status, removes the line item, and recalculates totals.</summary>
        /// <param name="command">The command containing the order and line item IDs.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            var lineItem = await dbContext.Set<LineItem>().FirstOrDefaultAsync(li => li.Id == command.LineItemId && li.OrderId == command.OrderId, cancellationToken);
            if (lineItem is null) return LineItemResult.Errors.NotFound(command.LineItemId);

            var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);
            if (order is null) return OrderResult.Errors.NotFound(command.OrderId);
            if (order.Status != OrderStatus.Draft) return OrderResult.Errors.InvalidStatusForLineItemRemove;
            // Remove: Delete entity from database.
            dbContext.Set<LineItem>().Remove(lineItem);
            order.RecalculateTotals();
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}
