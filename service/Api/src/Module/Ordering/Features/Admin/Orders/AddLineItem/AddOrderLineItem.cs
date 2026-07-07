using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

using Microsoft.EntityFrameworkCore;
namespace Module.Ordering.Features.Admin.Orders.AddLineItem;
/// <summary>Handles AddOrderLineItem feature.</summary>
public static partial class AddOrderLineItem
{
    public sealed record Command(Guid OrderId, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Response>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Create: Build the line item.
            var createResult = LineItemExtensions.Create(command.OrderId, command.Request.VariantId, command.Request.Quantity, command.Request.Price);
            if (createResult.IsFailure)
                return createResult.Failures;

            var lineItem = createResult.Value;

            // Persist: Add the new entity.
            dbContext.Set<LineItem>().Add(lineItem);

            // Update: Recalculate order totals.
            var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);
            if (order is not null)
            {
                order.RecalculateTotals();
            }

            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the created entity as response.
            return new Response { Id = lineItem.Id, VariantId = lineItem.VariantId, Quantity = lineItem.Quantity, Total = lineItem.Total };
        }
    }
}
