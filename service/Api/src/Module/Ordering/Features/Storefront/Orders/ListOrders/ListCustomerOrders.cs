using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Shared.Mappings;

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
            // Contract: pre=query!=null, post=result!=null
            var parameters = request.Parameters;

            // Check: Resolve current user identifier
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return PagedResult<Response>.Create();

            // Parse: Validate and parse querying parameters
            var parseAll = parameters.ParseAll(
                allowedFilterFields: OrderConstant.Query.AllowedFilterFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSearchFields: OrderConstant.Query.AllowedSearchFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSortFields: OrderConstant.Query.AllowedSortFields.ToHashSet(StringComparer.OrdinalIgnoreCase));
            if (parseAll.IsFailure)
                return parseAll.Errors;

            // Filter: Exclude draft orders from customer order list.
            var pagedResult = await dbContext.Set<Order>()
                .AsNoTracking()
                .Where(o => o.UserId == userId && o.Status != OrderStatus.Draft)
                .OrderByDescending(o => o.CreatedAtUtc)
                .ApplyQuerying(parseAll.Value)
                .ToPagedOrAllAsync(parseAll.Value, x => x.MapToStoreListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}