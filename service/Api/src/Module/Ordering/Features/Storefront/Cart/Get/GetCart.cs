using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.Get;

    /// <summary>Handles GetCart feature.</summary>
    public static partial class GetCart
{
    public sealed record Query : IQuery<Response>;

    public sealed class QueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Handles the query.</summary>
        /// <param name="query">The query to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the query.</returns>
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {

        // Contract: pre=query!=null, post=result!=null
            // Check: Resolve current user identifier or guest session.
            var userId = Guid.TryParse(currentUser.UserId, out var parsedId) ? parsedId : (Guid?)null;
            var sessionId = currentUser.IsAuthenticated ? null : currentUser.SessionId;

            if (userId is null && string.IsNullOrWhiteSpace(sessionId))
                return OrderResult.Errors.UserNotAuthenticated;

            // Query: Find the current user's active cart (Draft order).
            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .ThenInclude(x => x.Variant)
                .Where(x => (x.UserId == userId && x.Status == OrderStatus.Draft)
                         || (x.SessionId == sessionId && x.Status == OrderStatus.Draft))
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
            {
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

            return new Response
            {
                Id = cart.Id,
                Items = cart.LineItems.Select(li => new CartItem
                {
                    Id = li.Id,
                    VariantId = li.VariantId,
                    VariantName = li.Variant?.Sku ?? "",
                    Sku = li.Variant?.Sku ?? "",
                    Quantity = li.Quantity,
                    Price = li.Price,
                    Total = li.Total
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
