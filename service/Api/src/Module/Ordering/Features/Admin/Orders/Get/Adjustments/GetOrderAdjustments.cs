using Module.Ordering.Domain.Adjustments;

namespace Module.Ordering.Features.Admin.Orders.Get.Adjustments;
    /// <summary>Handles GetOrderAdjustments feature.</summary>
    public static partial class GetOrderAdjustments
{
    public sealed record Query(Guid OrderId, QueryingParameters Parameters) : IPagedQuery<Response>;
    public sealed class PagedQueryHandler(IApplicationDbContext dbContext) : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Handles the query.</summary>
        /// <param name="request">The query request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the query.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            var query = dbContext.Set<Adjustment>().AsNoTracking()
                .Where(a => a.OrderId == request.OrderId)
                .ApplyQueryOptions(parameters);

            var pagedResult = await query
                .Select(a => new Response { Id = a.Id, Label = a.Label, Amount = a.Amount, Eligible = a.Eligible, Included = a.Included, Mandatory = a.Mandatory, State = a.State, SourceId = a.SourceId, SourceType = a.SourceType, CreatedAtUtc = a.CreatedAtUtc })
                .ToPagedOrAllAsync(x => x, parameters, cancellationToken);

            return pagedResult;
        }
    }
}
