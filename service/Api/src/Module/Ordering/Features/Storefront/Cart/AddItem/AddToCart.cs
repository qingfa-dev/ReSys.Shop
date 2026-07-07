using Module.Catalog.Domain.Products.Variants;
using Module.Inventory.Domain.Stock;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.AddItem;

    /// <summary>Handles AddToCart feature.</summary>
    public static partial class AddToCart
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {

        // Contract: pre=command!=null, post=result!=null
            var request = command.Request;

            // Check: Resolve current user identifier or guest session.
            var userId = Guid.TryParse(currentUser.UserId, out var parsedId) ? parsedId : (Guid?)null;
            var sessionId = currentUser.IsAuthenticated ? null : currentUser.SessionId;

            if (userId is null && string.IsNullOrWhiteSpace(sessionId))
                return OrderResult.Errors.UserNotAuthenticated;

            // Check: Verify variant exists.
            var variant = await dbContext.Set<Variant>()
                .FirstOrDefaultAsync(x => x.Id == request.VariantId, cancellationToken);

            if (variant is null)
                return LineItemResult.Errors.VariantNotFound(request.VariantId);

            // Check: Find or create draft order for current user or guest session.
            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Where(x => (x.UserId == userId && x.Status == OrderStatus.Draft)
                         || (x.SessionId == sessionId && x.Status == OrderStatus.Draft))
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
            {
                var createResult = OrderExtensions.Create("USD", userId, Guid.Empty, sessionId: sessionId);
                if (createResult.IsFailure)
                    return createResult.Errors;

                cart = createResult.Value;
                // Create: Persist new entity.
                dbContext.Set<Order>().Add(cart);
            }

            // Validate: Check stock availability for requested quantity.
            var stockItems = await dbContext.Set<StockItem>()
                .Include(x => x.StockLocation)
                .Where(x => x.VariantId == request.VariantId)
                .ToListAsync(cancellationToken);

            // Validate: Check stock availability.
            if (!AvailabilityValidator.IsAvailable(stockItems, request.Quantity))
                return StockItemResult.Errors.InsufficientStock;

            // Check: If variant already in cart, update quantity.
            var existingLine = cart.LineItems.FirstOrDefault(li => li.VariantId == request.VariantId);
            if (existingLine is not null)
            {
                existingLine.Quantity += request.Quantity;
                existingLine.Total = existingLine.Price * existingLine.Quantity;
                cart.RecalculateTotals();
                // Persist: Save changes to the database.
                await dbContext.SaveChangesAsync(cancellationToken);
                return Result<Response>.Ok(new Response { LineItemId = existingLine.Id });
            }

            // Create: Add new line item.
            var lineItem = new LineItem
            {
                Id = Guid.NewGuid(),
                OrderId = cart.Id,
                VariantId = request.VariantId,
                Quantity = request.Quantity,
                Price = variant.Price ?? 0,
                Total = (variant.Price ?? 0) * request.Quantity,
                Currency = "USD",
                CreatedAtUtc = DateTimeOffset.UtcNow,
                CreatedBy = currentUser.UserName
            };

            // Create: Persist new entity.
            dbContext.Set<LineItem>().Add(lineItem);
            cart.RecalculateTotals();

            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record operation outcome.
            LineItemLoggers.Created(logger, Id: lineItem.Id, OrderId: cart.Id, VariantId: request.VariantId, ActionBy: currentUser.UserName);

            // Map: Return mapped result.
            return Result<Response>.Created(
                new Response { LineItemId = lineItem.Id },
                LineItemResult.Success.Created(lineItem.Id));
        }
    }
}
