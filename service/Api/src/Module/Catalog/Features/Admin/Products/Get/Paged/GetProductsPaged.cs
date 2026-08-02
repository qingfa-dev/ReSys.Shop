using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Admin.Products.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Products.Get.Paged;

/// <summary>
/// Defines the use case for retrieving paginated products.
/// </summary>
public static partial class GetProductsPagedList
{
    public record Query(Parameters Parameters) : IPagedQuery<Response>;

    /// <summary>
    /// Retrieves a paged list of non-deleted products with optional filtering
    /// by status, taxon classification, season, and search term.
    /// </summary>
    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>
        /// Retrieves a paged list of non-deleted products with optional filtering
        /// by status, taxon classification, season, and search term.
        /// </summary>
        /// <param name="request">The query containing filtering and pagination parameters.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A paged result of product list items.</returns>
        // Contract: pre=request.Parameters!=null, post=result.Items!=null
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            // Load: Base query excluding soft-deleted products with variant data included
            var query = dbContext.Set<Product>()
                .Include(x => x.Variants)
                .Where(x => !x.IsDeleted)
                .AsNoTracking();

            // Filter: By product status when specified
            if (parameters.Status.HasValue)
                query = query.Where(x => x.Status == parameters.Status.Value);

            // Filter: By taxon classification when specified
            if (parameters.TaxonId.HasValue)
                query = query.Where(x => x.Classifications.Any(c => c.TaxonId == parameters.TaxonId.Value));

            // Filter: By season (taxon name match) when specified
            if (!string.IsNullOrWhiteSpace(parameters.Season))
                query = query.Where(x => x.Classifications.Any(c => c.Taxon != null && c.Taxon.Name == parameters.Season));

            // Filter: By search term against product name (case-insensitive)
            if (!string.IsNullOrWhiteSpace(parameters.Search))
                query = query.Where(x => EF.Functions.ILike(x.Name, $"%{parameters.Search}%"));

            // Sort: Default newest-first ordering then apply pagination
            // Parse: Validate and parse querying parameters
            var parsing = parameters.ParseAll(
                allowedFilterFields: ProductConstant.Query.AllowedFilterFields,
                allowedSearchFields: ProductConstant.Query.AllowedSearchFields,
                allowedSortFields: ProductConstant.Query.AllowedSortFields);
            if (parsing.IsFailure)
                return parsing.Errors;

            var pagedResult = await query
                .Include(x => x.Classifications)
                .Include(x => x.Classifications).ThenInclude(c => c.Taxon)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}