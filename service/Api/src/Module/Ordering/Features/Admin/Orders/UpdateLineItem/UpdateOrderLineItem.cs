using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.UpdateLineItem;
/// <summary>Handles UpdateOrderLineItem feature.</summary>
public static partial class UpdateOrderLineItem
{
    public sealed record Command(Guid OrderId, Guid LineItemId, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Response>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Check: Find the line item.
            var lineItem = await dbContext.Set<LineItem>().FirstOrDefaultAsync(li => li.Id == command.LineItemId && li.OrderId == command.OrderId, cancellationToken);
            if (lineItem is null)
                return LineItemResult.Errors.NotFound(command.LineItemId);

            // Update: Apply quantity change.
            var updateResult = lineItem.UpdateQuantity(command.Request.Quantity);
            if (updateResult.IsFailure)
                return updateResult.Failures;

            // Update: Recalculate order totals.
            var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);
            if (order is not null)
                order.RecalculateTotals();

            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the updated entity as response.
            return new Response { Id = lineItem.Id, Quantity = lineItem.Quantity, Total = lineItem.Total };
        }
    }
}
