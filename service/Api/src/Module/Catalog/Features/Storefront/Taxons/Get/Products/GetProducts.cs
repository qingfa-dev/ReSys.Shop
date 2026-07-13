using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Storefront.Products.Shared.Mappings;

namespace Module.Catalog.Features.Storefront.Taxons.Get.Products;

/// <summary>
/// Defines the use case for retrieving products by taxon.
/// </summary>
public static partial class GetProducts
{
    public sealed record Query(Guid Id, Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>
        /// Retrieves a paged list of products belonging to a taxon and its descendants using nested set (Lft/Rgt) range.
        /// </summary>
        /// <param name="request">The query containing the taxon ID and pagination parameters.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A paged result of storefront product list items.</returns>
        // Contract: pre=request!=null, post=result!=null
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            var taxon = await dbContext.Set<Taxon>()
                .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken);

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

            var sortedQuery = query
                .OrderByDescending(x => x.CreatedAtUtc);

            var filteredQuery = sortedQuery.ApplyQuerying(parsing.Value);

            var page = parsing.Value.Page;

            List<Response> mapped;
            if (page.IsEmpty)
            {
                var allItems = await filteredQuery.AsNoTracking().ToListAsync(cancellationToken);
                mapped = allItems.Select(x => x.MapToStoreListItem<Response>()).ToList();
                return PagedResult<Response>.Create(mapped, 1, Math.Max(1, mapped.Count), mapped.Count);
            }

            var count = await filteredQuery.LongCountAsync(cancellationToken);

            var items = await filteredQuery
                .Skip(page.Skip)
                .Take(page.PageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            mapped = items.Select(x => x.MapToStoreListItem<Response>()).ToList();

            return PagedResult<Response>.Create(mapped, page.Page, page.PageSize, count);
        }
    }
}