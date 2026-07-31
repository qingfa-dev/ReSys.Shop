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
    public sealed record Query(Guid Id, Parameters Parameters) : IPagedQuery<Response>;

    /// <summary>
    /// Retrieves all option types with their assigned state and position for a product.
    /// </summary>
    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        // Contract: pre=request.Id!=Guid.Empty, post=result!=null
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
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
                return ot.MapToListItem<Response>(
                    isAssigned,
                    isAssigned ? assignedPositions[ot.Id] : 0);
            }).OrderBy(i => i.Position).ToList();

            var pageModel = PageModelExtensions.FromValues(request.Parameters.PageNumber, request.Parameters.PageSize).Value;
            return pageModel.IsEmpty
                ? PagedResult<Response>.Create(items, 1, Math.Max(1, items.Count), items.Count)
                : items.ToPagedResult(pageModel);
        }
    }
}
