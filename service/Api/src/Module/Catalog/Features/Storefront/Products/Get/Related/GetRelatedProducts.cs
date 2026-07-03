using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Storefront.Products.Shared.Mappings;
using Module.Catalog.Features.Storefront.Products.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Get.Related;

/// <summary>
/// Defines the use case for retrieving related products by shared taxons.
/// </summary>
public static partial class GetRelatedProducts
{
    public sealed record Query(Guid Id, Parameters Parameters) : IPagedQuery<Response>;

    public record Parameters : QueryingParameters;

public record Response : StoreProductListItemResponse;

    /// <summary>
    /// Retrieves related products for a given product using shared taxon strategy.
    /// Page size from parameters controls the number of related products returned.
    /// </summary>
    public sealed class PagedQueryHandler(
        IApplicationDbContext dbContext,
        ILogger<PagedQueryHandler> logger) : IPagedQueryHandler<Query, Response>
    {
        /// <inheritdoc />
        // Contract: pre=query.Id!=Guid.Empty, post=result.Items!=null
        public async Task<PagedResult<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var product = await dbContext.Set<Product>()
                .Include(x => x.Classifications)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == query.Id && !x.IsDeleted, cancellationToken);

            if (product is null)
            {
                // Log: Record product not found for observability
                ProductLoggers.StorefrontProductNotFoundById(logger, query.Id);
                return PagedResult<Response>.Ok([], 0, 0, 0);
            }

            var taxonIds = product.Classifications
                .Select(c => c.TaxonId)
                .ToList();

            if (taxonIds.Count == 0)
            {
                // Log: Record no taxons found for observability
                ProductLoggers.StorefrontNoTaxonsFound(logger, query.Id);
                return PagedResult<Response>.Ok([], 0, 0, 0);
            }

            var parameters = query.Parameters;

            var relatedQuery = dbContext.Set<Product>()
                .Include(x => x.Variants)
                    .ThenInclude(v => v.Prices)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.VariantImages)
                .Where(x => x.Id != query.Id
                    && !x.IsDeleted
                    && x.AvailableOn <= DateTimeOffset.UtcNow
                    && x.Classifications.Any(c => taxonIds.Contains(c.TaxonId)))
                .AsNoTracking();

            // Parse: Validate and parse querying parameters
            var parsing = parameters.ParseAll();
            if (parsing.IsFailure)
                return parsing.Errors;

            var pagedResult = await relatedQuery
                .OrderByDescending(x => x.Classifications.Count(c => taxonIds.Contains(c.TaxonId)))
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToStoreListItem<Response>(), cancellationToken);

            // Log: Record related products found for observability
            ProductLoggers.StorefrontRelatedProductsFound(logger, (int)pagedResult.TotalCount, query.Id);

            return pagedResult;
        }
    }
}
