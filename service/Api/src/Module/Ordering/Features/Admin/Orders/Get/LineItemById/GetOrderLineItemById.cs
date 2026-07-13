using Module.Ordering.Domain.LineItems;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;

namespace Module.Ordering.Features.Admin.Orders.Get.LineItemById;

/// <summary>Retrieves a single line item by ID scoped to its parent order, returning a detail response DTO.</summary>
public static partial class GetOrderLineItemById
{
    public sealed record Query(Guid OrderId, Guid LineItemId) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext) : IQueryHandler<Query, Response>
    {
        /// <summary>Finds the line item by IDs and maps it to a response DTO.</summary>
        /// <param name="query">The query containing order ID and line item ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The line item detail response.</returns>
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Contract: pre=query!=null, post=result!=null
            // Check: Find the line item scoped to its parent order.
            var lineItem = await dbContext.Set<LineItem>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    li => li.Id == query.LineItemId && li.OrderId == query.OrderId,
                    cancellationToken);

            if (lineItem is null)
                return LineItemResult.Errors.NotFound(query.LineItemId);

            return lineItem.MapToLineItemResponse<Response>();
        }
    }
}