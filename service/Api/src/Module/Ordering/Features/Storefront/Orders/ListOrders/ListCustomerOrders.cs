using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Orders.ListOrders;

/// <summary>Lists the current customer's placed orders (excluding drafts) with paging, filtering, and sorting.</summary>
public static partial class ListCustomerOrders
{
    public sealed record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Parses parameters and queries placed orders scoped to the current user with paging.</summary>
        /// <param name="request">The paged query request with parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The paged order list response.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            // Contract: pre=query!=null, post=result!=null
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return PagedResult<Response>.Create();

            var parseAll = parameters.ParseAll();
            if (parseAll.IsFailure)
                return parseAll.Errors;

            // Check: Retrieve orders for current user (excluding drafts) with querying options.
            var pagedResult = await dbContext.Set<Order>()
                .AsNoTracking()
                .Where(o => o.UserId == userId && o.Status != OrderStatus.Draft)
                .OrderByDescending(o => o.CreatedAtUtc)
                .ApplyQuerying(parseAll.Value)
                .Select(o => new Response
                {
                    Id = o.Id,
                    Number = o.Number,
                    Status = o.Status.ToString(),
                    Total = o.Total,
                    CreatedAtUtc = o.CreatedAtUtc
                })
                .ToPagedOrAllAsync(parseAll.Value, x => x, cancellationToken);

            return pagedResult;
        }
    }
}
