using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Admin.Products.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Products.Get.ById;

/// <summary>
/// Defines the use case for retrieving a product by ID.
/// </summary>
public static partial class GetProductById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    /// <summary>
    /// Retrieves a single product by ID with full related data including
    /// variants, prices, images, option types, and taxon classifications.
    /// </summary>
    public sealed class QueryHandler(
        IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>
        /// Handles the query — loads the product with 5 Include chains
        /// (variants, prices, images, options, classifications) and maps to DTO.
        /// </summary>
        /// <param name="query">The query containing the product ID.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result with the full product detail.</returns>
        // Contract: pre=query.Id!=Guid.Empty, post=result!=null
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Load: Fetch product with full related data (variants, prices, variant images, option types, taxon classifications)
            var entity = await dbContext.Set<Product>()
                .Include(x => x.Variants)
                    .ThenInclude(v => v.Prices)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.VariantImages)
                .Include(x => x.ProductOptionTypes)
                    .ThenInclude(po => po.OptionType)
                .Include(x => x.Classifications)
                    .ThenInclude(c => c.Taxon)
                .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);

            if (entity is null)
                return ProductResult.Errors.NotFound(query.Id);

            return Result<Response>.Ok(
                entity.MapToDetail<Response>());
        }
    }
}
