using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Variants;

using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.Shared.Mappings;

using Module.Inventory.Features.Shared;
using Module.Inventory.Services;
using Module.Inventory.Services.StockReservations;

namespace Module.Ordering.Features.Storefront.Cart.UpdateItemQuantity;
/// <summary>Updates the quantity of a line item in the current user's draft cart after validating stock availability.</summary>
public static partial class UpdateCartItemQuantity
{
    public sealed record Command(Guid LineItemId, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser,
        IStockItemService stockItem,
        IStockReservationService stockReservationService)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Validates stock via Inventory module, releases the prior reservation, re-reserves the new quantity, updates the line item, and recalculates cart totals.</summary>
        /// <param name="command">The command containing the line item ID and new quantity.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Errors.UserNotAuthenticated;

            if (command.Request.Quantity <= 0 || command.Request.Quantity > LineItemConstant.MaxQuantity)
                return OrderResult.Errors.QuantityNotPositive;

            // Check: Find the user's draft cart.
            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Where(x => x.UserId == userId && x.Status == OrderStatus.Draft)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
                return OrderResult.Errors.NotFound(Guid.Empty);

            var lineItem = cart.LineItems.FirstOrDefault(li => li.Id == command.LineItemId);
            if (lineItem is null)
                return LineItemResult.Errors.NotFound(command.LineItemId);

            // Validate: Check reservation-aware stock availability directly via service
            var availableResult = await stockItem.IsAvailableAsync(
                lineItem.VariantId, command.Request.Quantity, ct: cancellationToken);

            if (!availableResult.IsSuccess || !availableResult.Value)
                return OrderResult.Errors.CartQuantityInvalid;

            // Release: Free the line item's prior reservation so the re-reserve reflects the new quantity.
            var releaseResult = await stockReservationService.ReleaseCartReservationsAsync(
                cart.Id.ToString(), lineItem.VariantId, cancellationToken);
            if (releaseResult.IsFailure)
                return releaseResult.Errors;

            // Reserve: Hold the updated quantity for this cart.
            var reserveResult = await stockReservationService.ReserveForVariantAsync(
                lineItem.VariantId, command.Request.Quantity, cartToken: cart.Id.ToString(),
                ttlMinutes: InventoryFeature.Storefront.StockReservations.TtlMinutesDefault, ct: cancellationToken);
            if (reserveResult.IsFailure)
                return reserveResult.Errors;

            // Update: Modify quantity and total.
            var previousTotal = cart.Total;
            var updateResult = lineItem.UpdateQuantity(command.Request.Quantity);
            if (updateResult.IsFailure)
                return updateResult.Errors;
            var recalcResult = cart.RecalculateTotals();
            if (recalcResult.IsFailure)
                return recalcResult.Errors;

            cart.RegressCheckoutIfAmountChanged(previousTotal);

            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record quantity change in audit log.
            LineItemLoggers.QuantityUpdated(logger, Id: lineItem.Id, OrderId: cart.Id, Quantity: lineItem.Quantity, ActionBy: currentUser.UserName);

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
