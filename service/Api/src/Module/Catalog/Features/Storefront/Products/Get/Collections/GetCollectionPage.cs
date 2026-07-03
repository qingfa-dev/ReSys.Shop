using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Storefront.Products.Shared.Mappings;

namespace Module.Catalog.Features.Storefront.Products.Get.Collections;

/// <summary>
/// Defines the use case for retrieving a collection page of products.
/// </summary>
public static partial class GetCollectionPage
{
    public sealed record Query(string Season, Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>
        /// Handles the collection page query — retrieves products filtered by season
        /// (via taxon classification) with configurable sorting and pagination.
        /// </summary>
        /// <param name="request">The query containing season, sort, and pagination parameters.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A paged result of collection product list items.</returns>
        // Contract: pre=request.Parameters!=null, post=result.Items!=null
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            var query = dbContext.Set<Product>()
                .Include(x => x.Variants)
                    .ThenInclude(v => v.Prices)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.VariantImages)
                .Where(x => !x.IsDeleted
                    && x.AvailableOn <= DateTimeOffset.UtcNow
                    && x.Classifications.Any(c => c.Taxon != null && c.Taxon.Name == request.Season))
                .AsNoTracking();

            // Parse: Validate and parse querying parameters
            var parsing = parameters.ParseAll();
            if (parsing.IsFailure)
                return parsing.Errors;

            var pagedResult = await query
                .OrderByDescending(x => x.AvailableOn ?? x.CreatedAtUtc)
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToStoreListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}
