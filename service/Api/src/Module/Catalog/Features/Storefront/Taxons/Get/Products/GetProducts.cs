using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Storefront.Products.Shared.Mappings;

namespace Module.Catalog.Features.Storefront.Taxons.Get.Products;

/// <summary>
/// Defines the use case for retrieving products by taxon.
/// </summary>
public static partial class GetProducts
{
    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>
        /// Handles the request and returns a result.
        /// </summary>
        /// <param name="request">The query containing request data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        // Contract: pre=request!=null, post=result!=null
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            var taxon = await dbContext.Set<Taxon>()
                .FirstOrDefaultAsync(t => t.Id == parameters.TaxonId && !t.IsDeleted, cancellationToken);

            if (taxon is null)
                return PagedResult<Response>.Ok([], 0, 0, 0);

            var query = dbContext.Set<Product>()
                .Include(x => x.Variants)
                    .ThenInclude(v => v.Prices)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.VariantImages)
                .Where(x => !x.IsDeleted
                    && x.AvailableOn <= DateTimeOffset.UtcNow
                    && x.Classifications.Any(c => c.Taxon != null
                        && c.Taxon.Lft >= taxon.Lft
                        && c.Taxon.Rgt <= taxon.Rgt
                        && c.Taxon.TaxonomyId == taxon.TaxonomyId))
                .AsNoTracking();

            // Parse: Validate and parse querying parameters
            var parsing = parameters.ParseAll();
            if (parsing.IsFailure)
                return parsing.Errors;

            var pagedResult = await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToStoreListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}
