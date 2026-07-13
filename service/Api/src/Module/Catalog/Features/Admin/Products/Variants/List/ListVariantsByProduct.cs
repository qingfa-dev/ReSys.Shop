using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Features.Admin.Products.Variants.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Products.Variants.List;

/// <summary>
/// Defines the use case for listing variants by product.
/// </summary>
public static partial class ListVariantsByProduct
{
    public sealed record Query(Guid ProductId) : IQuery<Response>;

    /// <summary>
    /// Lists all non-deleted variants for a product, including prices,
    /// option-value associations, and images.
    /// </summary>
    public sealed class QueryHandler(
        IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>
        /// Handles the list-variants query — loads non-deleted variants for the
        /// given product with prices, options, and images eagerly loaded,
        /// then maps each to a detail DTO.
        /// </summary>
        /// <param name="query">The query containing the parent product ID.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result with the list of variant items.</returns>
        // Contract: pre=query.ProductId!=Guid.Empty, post=result.Items!=null
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Load: Fetch non-deleted variants for product with prices, option value assocs, and images
            var variants = await dbContext.Set<Variant>()
                .Include(x => x.Prices)
                .Include(x => x.OptionValueVariants)
                    .ThenInclude(ovv => ovv.OptionValue)
                .Include(x => x.VariantImages)
                .Where(x => x.ProductId == query.ProductId && !x.IsDeleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Map: Project each variant to detail DTO with all relations
            var items = variants
                .Select(v => v.MapToDetail<Response.Item>())
                .ToList();

            return Result<Response>.Ok(new Response { Items = items });
        }
    }
}