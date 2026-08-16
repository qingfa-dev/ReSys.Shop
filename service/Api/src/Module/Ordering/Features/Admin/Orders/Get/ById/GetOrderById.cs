using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Shared.Mappings;
using Module.Ordering.Features.Storefront.Shared.Services;

namespace Module.Ordering.Features.Admin.Orders.Get.ById;

/// <summary>Retrieves a single order by its unique identifier, including line items, for admin viewing.</summary>
public static partial class GetOrderById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Finds the order by ID with included line items and maps it to a detail response.</summary>
        /// <param name="query">The query containing the order ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The order detail response.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=query!=null, post=result!=null
            // Check: Find the order by identifier with line items.
            var entity = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Include(x => x.Adjustments)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity is null)
                return OrderResult.Errors.NotFound(request.Id);

            // Enrich: Resolve product references (id, name, primary image) for the order line items.
            var variantIds = entity.LineItems.Select(li => li.VariantId).Distinct().ToList();
            var itemLookup = await ProductLookupFactory.BuildAsync(dbContext, variantIds, cancellationToken);

            // Map: Convert entity to response DTO with enriched line items.
            return entity.MapToDetailWithLookup<Response>(itemLookup);
        }
    }
}