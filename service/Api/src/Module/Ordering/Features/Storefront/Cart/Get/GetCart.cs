using Module.Catalog.Domain.Products.Variants;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.Get;

/// <summary>Retrieves the current user's active cart (draft order) with line items and totals, or returns an empty cart structure.</summary>
public static partial class GetCart
{
    public sealed record Query : IQuery<Response>;

    public sealed class QueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Finds the current user's active draft cart with included line items, returning an empty cart if none exists.</summary>
        /// <param name="query">The (empty) query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The cart response or an empty cart structure.</returns>
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Contract: pre=query!=null, post=result!=null
            // Check: Resolve current user identifier or guest session.
            var userId = Guid.TryParse(currentUser.UserId, out var parsedId) ? parsedId : (Guid?)null;
            var sessionId = currentUser.IsAuthenticated ? null : currentUser.SessionId;

            if (userId is null && string.IsNullOrWhiteSpace(sessionId))
                return OrderResult.Errors.UserNotAuthenticated;

            // Check: Find the current user's active cart (Draft order).
            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Where(x => (x.UserId == userId && x.Status == OrderStatus.Draft)
                         || (x.SessionId == sessionId && x.Status == OrderStatus.Draft))
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
            {
                // Map: Return empty cart structure when no cart exists.
                return new Response
                {
                    Items = [],
                    ItemTotal = 0,
                    Total = 0,
                    Currency = "USD",
                    ItemCount = 0,
                    CheckoutState = string.Empty
                };
            }

            // Map: Enrich line items with variant details (name, SKU) from catalog.
            var variantIds = cart.LineItems.Select(li => li.VariantId).ToList();
            var variants = await dbContext.Set<Variant>()
                .Where(v => variantIds.Contains(v.Id))
                .AsNoTracking()
                .ToDictionaryAsync(v => v.Id, v => v, cancellationToken);

            return new Response
            {
                Id = cart.Id,
                Items = cart.LineItems.Select(li =>
                {
                    variants.TryGetValue(li.VariantId, out var v);
                    return new CartItem
                    {
                        Id = li.Id,
                        VariantId = li.VariantId,
                        VariantName = v?.Sku ?? "",
                        Sku = v?.Sku ?? "",
                        Quantity = li.Quantity,
                        Price = li.Price,
                        Total = li.Total
                    };
                }).ToList(),
                ItemTotal = cart.ItemTotal,
                Total = cart.Total,
                Currency = cart.Currency,
                ItemCount = cart.ItemCount,
                CheckoutState = cart.CheckoutState.ToString()
            };
        }
    }
}
