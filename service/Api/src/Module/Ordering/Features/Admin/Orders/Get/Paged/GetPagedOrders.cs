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
            var parsing = request.Parameters.ParseAll();
            if (parsing.IsFailure)
                return parsing.Errors;

            var pagedResult = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .AsNoTracking()
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}
