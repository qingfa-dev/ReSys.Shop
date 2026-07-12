using Microsoft.Extensions.Configuration;

using Module.Catalog.Domain.Products.Variants;
using Module.Inventory.Domain.Stock;
using Module.Inventory.Features.Storefront.CartReservations.Reserve;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.AddItem;

/// <summary>Adds a variant to the current user's cart, creating a new draft order if none exists, merging with existing line items for the same variant.</summary>
public static partial class AddToCart
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser,
        IConfiguration configuration,
        ISender sender)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Adds a variant to the user's cart, creating a new cart or merging with an existing line item, with stock validation.</summary>
        /// <param name="command">The command containing the variant ID and quantity.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The response with the new or updated line item ID.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            var request = command.Request;

            // Check: Resolve current user identifier or guest session.
            var userId = Guid.TryParse(currentUser.UserId, out var parsedId) ? parsedId : (Guid?)null;
            var sessionId = currentUser.IsAuthenticated ? null : currentUser.SessionId;

            if (userId is null && string.IsNullOrWhiteSpace(sessionId))
                return OrderResult.Errors.UserNotAuthenticated;

            // Check: Variant exists in catalog — reject unknown products.
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
                // Create: New draft cart with default currency from configuration.
                var currency = configuration["Ordering:DefaultCurrency"] ?? "USD";
                var createResult = OrderExtensions.Create(currency, userId, Guid.Empty, sessionId: sessionId, shipAddressId: null);
                if (createResult.IsFailure)
                    return createResult.Errors;

                cart = createResult.Value;
                dbContext.Set<Order>().Add(cart);
            }

            // Validate: Stock availability for requested quantity.
            var stockItems = await dbContext.Set<StockItem>()
                .Include(x => x.StockLocation)
                .Where(x => x.VariantId == request.VariantId)
                .ToListAsync(cancellationToken);

            if (!AvailabilityValidator.IsAvailable(stockItems, request.Quantity))
                return StockItemResult.Errors.InsufficientStock;

            // Reserve: Lock stock for the cart duration (30-min TTL).
            var primaryLocation = stockItems
                .Where(si => si.CountOnHand > 0)
                .OrderByDescending(si => si.CountOnHand)
                .FirstOrDefault();

            if (primaryLocation is not null)
            {
                var cartToken = currentUser.IsAuthenticated
                    ? currentUser.UserId!
                    : currentUser.SessionId ?? string.Empty;

                var reserveResult = await sender.Send(
                    new ReserveCartStock.Command(
                        new ReserveCartStock.Request
                        {
                            VariantId = request.VariantId,
                            Quantity = request.Quantity,
                            StockLocationId = primaryLocation.StockLocationId,
                            TtlMinutes = 30
                        },
                        cartToken),
                    cancellationToken);

                if (reserveResult.IsFailure)
                    return reserveResult.Errors;
            }

            // Merge: Variant already in cart — add to existing line item quantity.
            var existingLine = cart.LineItems.FirstOrDefault(li => li.VariantId == request.VariantId);
            if (existingLine is not null)
            {
                // Validate: Combined quantity must not exceed per-line maximum.
                if (existingLine.Quantity + request.Quantity > LineItemConstant.MaxQuantity)
                    return LineItemResult.Errors.QuantityExceedsMax;
                // Update: Increment existing line item quantity and recalculate.
                existingLine.Quantity += request.Quantity;
                existingLine.Total = existingLine.Price * existingLine.Quantity;
                cart.RecalculateTotals();
                await dbContext.SaveChangesAsync(cancellationToken);
                return Result<Response>.Ok(new Response { LineItemId = existingLine.Id });
            }

            // Create: Add new line item to cart with variant price snapshot.
            var lineItem = LineItemMethod.Create(cart.Id, request.VariantId, request.Quantity, variant.Price ?? 0);
            if (lineItem.IsFailure)
                return lineItem.Errors;

            var newItem = lineItem.Value;

            dbContext.Set<LineItem>().Add(newItem);
            cart.RecalculateTotals();

            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record the new line item in audit log.
            LineItemLoggers.Created(logger, Id: newItem.Id, OrderId: cart.Id, VariantId: request.VariantId, ActionBy: currentUser.UserName);

            return Result<Response>.Created(
                new Response { LineItemId = newItem.Id },
                LineItemResult.Success.Created(newItem.Id));
        }
    }
}
