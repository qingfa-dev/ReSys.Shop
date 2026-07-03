using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Options;
using Module.Catalog.Features.Admin.Products.OptionTypes.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Products.OptionTypes.Get;

/// <summary>
/// Defines the use case for retrieving product option types with assigned state.
/// </summary>
public static partial class GetProductOptionTypes
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(
        IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>
        /// Handles the request and returns a result.
        /// </summary>
        /// <param name="request">The query containing request data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        // Contract: pre=request!=null, post=result!=null
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Check: Product exists before retrieving option types
            var productExists = await dbContext.Set<Product>()
                .AnyAsync(x => x.Id == request.Id, cancellationToken);
            if (!productExists)
                return ProductResult.Errors.NotFound(request.Id);

            // Load: All available option types
            var allOptionTypes = await dbContext.Set<OptionType>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Load: Position map for assigned option types
            var assignedPositions = await dbContext.Set<ProductOptionType>()
                .Where(x => x.ProductId == request.Id)
                .ToDictionaryAsync(x => x.OptionTypeId, x => x.Position, cancellationToken);

            // Compute: Map each option type with IsAssigned flag and Position
            var items = allOptionTypes.Select(ot =>
            {
                var isAssigned = assignedPositions.ContainsKey(ot.Id);
                return ot.MapToListItem<Response.OptionTypeItem>(
                    isAssigned,
                    isAssigned ? assignedPositions[ot.Id] : 0);
            }).ToList();

            return new Response { Items = items };
        }
    }
}
