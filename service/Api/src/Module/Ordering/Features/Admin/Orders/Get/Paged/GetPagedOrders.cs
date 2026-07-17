using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;

namespace Module.Ordering.Features.Admin.Orders.Get.Paged;

/// <summary>Retrieves a paged, filterable, sortable list of orders for the admin order grid.</summary>
public static partial class GetPagedOrders
{
    public record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Parses query parameters and executes a paged database query with includes and mapping.</summary>
        /// <param name="request">The paged query request with parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The paged order list response.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            // Validate: Parse and validate paging/filtering/sorting parameters.
            var parsing = request.Parameters.ParseAll(
                allowedFilterFields: OrderConstant.Query.AllowedFilterFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSearchFields: OrderConstant.Query.AllowedSearchFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSortFields: OrderConstant.Query.AllowedSortFields.ToHashSet(StringComparer.OrdinalIgnoreCase));
            if (parsing.IsFailure)
                return parsing.Errors;

            // Map: Apply filters and project to list-item DTO.
            var pagedResult = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .AsNoTracking()
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}