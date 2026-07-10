using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.RemoveLineItem;
    /// <summary>Handles RemoveOrderLineItem feature.</summary>
    public static partial class RemoveOrderLineItem
{
    public sealed record Command(Guid OrderId, Guid LineItemId) : ICommand;
    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var lineItem = await dbContext.Set<LineItem>().FirstOrDefaultAsync(li => li.Id == command.LineItemId && li.OrderId == command.OrderId, cancellationToken);
            if (lineItem is null) return LineItemResult.Errors.NotFound(command.LineItemId);

            var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);
            if (order is null) return OrderResult.Errors.NotFound(command.OrderId);
            if (order.Status != OrderStatus.Draft) return Error.Validation("Order.RemoveLineItem.InvalidStatus", "Line items can only be removed from Draft orders.");
            // Remove: Delete entity from database.
            dbContext.Set<LineItem>().Remove(lineItem);
            order.RecalculateTotals();
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}
