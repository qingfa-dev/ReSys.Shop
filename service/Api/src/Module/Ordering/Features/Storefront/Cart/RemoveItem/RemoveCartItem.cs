using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Variants;

using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.Shared.Mappings;
using Module.Inventory.Services.StockReservations;

namespace Module.Ordering.Features.Storefront.Cart.RemoveItem;

/// <summary>Removes a line item from the current user's draft cart, releases its stock reservation, and recalculates order totals.</summary>
public static partial class RemoveCartItem
{
    public sealed record Command(Guid LineItemId) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IStockReservationService stockReservationService)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Finds the user's draft cart, removes the specified line item, releases its stock reservation, and recalculates totals.</summary>
        /// <param name="command">The command containing the line item ID to remove.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            var userId = Guid.TryParse(currentUser.UserId, out var parsed) ? parsed : (Guid?)null;
            var sessionId = currentUser.IsAuthenticated ? null : currentUser.SessionId;

            if (userId is null && string.IsNullOrWhiteSpace(sessionId))
                return OrderResult.Errors.UserNotAuthenticated;

            // Check: Find the user's draft cart.
            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Include(x => x.Adjustments)
                .Where(x => (x.UserId == userId && x.Status == OrderStatus.Draft)
                         || (x.SessionId == sessionId && x.Status == OrderStatus.Draft))
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
                return OrderResult.Errors.NotFound(Guid.Empty);

            // Check: Find the line item within the cart's collection.
            var lineItem = cart.LineItems.FirstOrDefault(li => li.Id == command.LineItemId);
            if (lineItem is null)
                return LineItemResult.Errors.NotFound(command.LineItemId);

            // Remove: Use domain method to remove from collection and recalculate, then delete from database for EF tracking.
            var previousTotal = cart.Total;
            var removeResult = cart.RemoveLineItem(command.LineItemId);
            if (removeResult.IsFailure)
                return removeResult.Errors;

            dbContext.Set<LineItem>().Remove(removeResult.Value);
            cart.RegressCheckoutIfAmountChanged(previousTotal);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Release: Free the removed item's stock reservation for this cart.
            var releaseResult = await stockReservationService.ReleaseCartReservationsAsync(
                cart.Id.ToString(), lineItem.VariantId, cancellationToken);
            if (releaseResult.IsFailure)
                return releaseResult.Errors;

            var variantIds = cart.LineItems.Select(li => li.VariantId).ToList();
            var itemLookup = await BuildCartItemLookupAsync(dbContext, variantIds, cancellationToken);
            return Result<Response>.Ok(cart.MapToDetailWithItems<Response>(itemLookup));
        }

        /// <summary>Builds the enriched cart item lookup (sku, product name, primary image) for the given variant ids.</summary>
        private static async Task<Dictionary<Guid, CartItemLookup>> BuildCartItemLookupAsync(
            IApplicationDbContext dbContext,
            IReadOnlyCollection<Guid> variantIds,
            CancellationToken cancellationToken)
        {
            if (variantIds.Count == 0)
                return new Dictionary<Guid, CartItemLookup>();

            var variants = await dbContext.Set<Variant>()
                .Where(v => variantIds.Contains(v.Id))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var productIds = variants.Select(v => v.ProductId).Distinct().ToList();
            var products = await dbContext.Set<Product>()
                .Where(p => productIds.Contains(p.Id))
                .Include(p => p.Variants)
                    .ThenInclude(v => v.VariantImages)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var productsById = products.ToDictionary(p => p.Id);

            return variants.ToDictionary(v => v.Id, v =>
            {
                if (!productsById.TryGetValue(v.ProductId, out var product))
                    return new CartItemLookup { Sku = v.Sku ?? string.Empty };

                var masterVariant = product.Variants.FirstOrDefault(x => x.IsMaster);
                var primaryImageUrl = (masterVariant?.VariantImages.OrderBy(i => i.Position).FirstOrDefault()
                    ?? product.Variants.SelectMany(x => x.VariantImages).OrderBy(i => i.Position).FirstOrDefault())
                    ?.Url;

                return new CartItemLookup
                {
                    Sku = v.Sku ?? string.Empty,
                    ProductName = product.Name,
                    ProductImageUrl = primaryImageUrl,
                };
            });
        }
    }
}