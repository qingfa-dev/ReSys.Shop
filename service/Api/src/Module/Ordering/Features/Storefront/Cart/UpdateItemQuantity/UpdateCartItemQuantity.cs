using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Inventory.Domain.Stock;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.UpdateItemQuantity;
/// <summary>Handles UpdateCartItemQuantity feature.</summary>
public static partial class UpdateCartItemQuantity
{
    public sealed record Command(Guid LineItemId, Request Request) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Errors.UserNotAuthenticated;

            if (command.Request.Quantity <= 0)
                return OrderResult.Errors.QuantityNotPositive;

            // Query: Find the user's draft cart.
            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Where(x => x.UserId == userId && x.Status == OrderStatus.Draft)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
                return OrderResult.Errors.NotFound(Guid.Empty);

            var lineItem = cart.LineItems.FirstOrDefault(li => li.Id == command.LineItemId);
            if (lineItem is null)
                return LineItemResult.Errors.NotFound(command.LineItemId);

            // Validate: Check stock availability.
            var stockItems = await dbContext.Set<StockItem>()
                .Where(x => x.VariantId == lineItem.VariantId)
                .ToListAsync(cancellationToken);

            if (!AvailabilityValidator.IsAvailable(stockItems, command.Request.Quantity))
                return StockItemResult.Errors.InsufficientStock;

            // Update: Modify quantity and total.
            lineItem.Quantity = command.Request.Quantity;
            lineItem.Total = lineItem.Price * command.Request.Quantity;
            cart.RecalculateTotals();

            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record operation.
            LineItemLoggers.QuantityUpdated(logger, Id: lineItem.Id, OrderId: cart.Id, Quantity: lineItem.Quantity, ActionBy: currentUser.UserName);

            return Result.Ok();
        }
    }
}
