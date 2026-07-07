using BuildingBlocks.Querying.Extensions;
using Microsoft.EntityFrameworkCore;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;

namespace Module.Ordering.Features.Admin.Orders.Get.Paged;

    /// <summary>Handles GetPagedOrders feature.</summary>
    public static partial class GetPagedOrders
{
    public record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Handles the paged query.</summary>
        /// <param name="request">The query request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The paged result.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {

        // Contract: pre=command!=null, post=result!=null
            var parameters = request.Parameters;

            // Query: Retrieve orders with line items, apply querying options, map to paged result.
            var pagedResult = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .AsNoTracking()
                .ApplyQueryOptions(parameters)
                .ToPagedOrAllAsync(x => x.MapToListItem<Response>(), parameters, cancellationToken);

            return pagedResult;
        }
    }
}
