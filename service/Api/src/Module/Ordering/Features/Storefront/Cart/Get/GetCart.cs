using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Shared.Mappings;
using Module.Ordering.Features.Storefront.Shared.Services;

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

            // Load: Find the current user's active draft cart with line items
            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Include(x => x.Adjustments)
                .Where(x => (x.UserId == userId && x.Status == OrderStatus.Draft)
                         || (x.SessionId == sessionId && x.Status == OrderStatus.Draft))
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            // Check: Return empty cart if none exists
            if (cart is null)
                return CartMapping.EmptyCart<Response>();

            // Enrich: Look up variant skus, product names, and primary images for line items
            var variantIds = cart.LineItems.Select(li => li.VariantId).ToList();
            var itemLookup = await ProductLookupFactory.BuildAsync(dbContext, variantIds, cancellationToken);

            // Map: Return cart with enriched line items
            return cart.MapToDetailWithItems<Response>(itemLookup);
        }
    }
}