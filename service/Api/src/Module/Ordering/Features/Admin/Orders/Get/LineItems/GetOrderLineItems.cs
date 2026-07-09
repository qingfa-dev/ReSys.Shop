using Module.Ordering.Domain.LineItems;

namespace Module.Ordering.Features.Admin.Orders.Get.LineItems;

/// <summary>Handles GetOrderLineItems feature.</summary>
public static partial class GetOrderLineItems
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

            var parseAll = parameters.ParseAll();
            if (parseAll.IsFailure)
                return parseAll.Errors;

            var query = dbContext.Set<LineItem>().AsNoTracking()
                .Where(li => li.OrderId == request.OrderId)
                .ApplyQuerying(parseAll.Value);

            var pagedResult = await query
                .Select(li => new Response
                {
                    Id = li.Id,
                    VariantId = li.VariantId,
                    Quantity = li.Quantity,
                    Price = li.Price,
                    Total = li.Total,
                    AdjustmentTotal = li.AdjustmentTotal,
                    Currency = li.Currency,
                    CreatedAtUtc = li.CreatedAtUtc
                })
                .ToPagedOrAllAsync(parseAll.Value, x => x, cancellationToken);

            return pagedResult;
        }
    }
}
