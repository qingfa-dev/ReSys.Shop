using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Products.Classifications.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Products.Classifications.Get;

/// <summary>
/// Defines the use case for retrieving product classifications with assigned state.
/// </summary>
public static partial class GetProductClassifications
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(
        IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>
        /// Retrieves all taxons for a product with their assigned state and position for the classification tree view.
        /// </summary>
        /// <param name="request">The query containing the product ID.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result with the product classification tree.</returns>
        // Contract: pre=request.Id!=Guid.Empty, post=result!=null
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Check: Product exists before retrieving classifications
            var productExists = await dbContext.Set<Product>()
                .AnyAsync(x => x.Id == request.Id, cancellationToken);
            if (!productExists)
                return ProductResult.Errors.NotFound(request.Id);

            // Load: All taxons for the full classification tree view
            var allTaxons = await dbContext.Set<Taxon>()
                .AsNoTracking()
                .Include(x => x.Taxonomy)
                .ToListAsync(cancellationToken);

            // Load: Position map for assigned classifications
            var assignedPositions = await dbContext.Set<Classification>()
                .Where(x => x.ProductId == request.Id)
                .Where(x => x.TaxonId != null)
                .ToDictionaryAsync(x => x.TaxonId!.Value, x => x.Position, cancellationToken);

            // Compute: Map each taxon with IsAssigned flag and Position
            var items = allTaxons.Select(t =>
            {
                var isAssigned = assignedPositions.ContainsKey(t.Id);
                return t.MapToListItem<Response.ClassificationItem>(
                    isAssigned,
                    isAssigned ? assignedPositions[t.Id] : 0);
            }).ToList();

            return new Response { Items = items };
        }
    }
}
