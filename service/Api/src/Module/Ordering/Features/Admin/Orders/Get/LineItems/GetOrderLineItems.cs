using Module.Ordering.Domain.LineItems;

namespace Module.Ordering.Features.Admin.Orders.Get.LineItems;

/// <summary>Retrieves a paged list of line items for a given order, supporting filtering and sorting for the admin view.</summary>
public static partial class GetOrderLineItems
{
    public sealed record Query(Guid OrderId, QueryingParameters Parameters) : IPagedQuery<Response>;
    public sealed class PagedQueryHandler(IApplicationDbContext dbContext) : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Parses parameters and queries line items scoped to the order with paging.</summary>
        /// <param name="request">The paged query request with order ID and parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The paged line item list response.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            var parameters = request.Parameters;

            // Validate: Parse and validate paging/filtering parameters.
            var parseAll = parameters.ParseAll();
            if (parseAll.IsFailure)
                return parseAll.Errors;

            // Filter: Scoped to the parent order's line items.
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
