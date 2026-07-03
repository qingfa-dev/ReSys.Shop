using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Storefront.Products.Get.Search.Shared.Mappings;

namespace Module.Catalog.Features.Storefront.Products.Get.Search;

/// <summary>
/// Defines the use case for searching products by text query.
/// </summary>
public static partial class SearchProducts
{
    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    /// <summary>
    /// Performs full-text search across products by name, slug, and description.
    /// Uses ILIKE for case-insensitive matching and returns scored results with pagination.
    /// </summary>
    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>
        /// Handles the search query — filters products by search term using ILIKE,
        /// applies pagination, and maps to search result DTOs.
        /// </summary>
        /// <param name="request">The query containing search term and pagination parameters.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A paged result of search products.</returns>
        // Contract: pre=request.Parameters!=null, post=result.Items!=null
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            var query = dbContext.Set<Product>()
                .Include(x => x.Variants)
                    .ThenInclude(v => v.Prices)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.VariantImages)
                .Where(x => !x.IsDeleted && x.AvailableOn <= DateTimeOffset.UtcNow)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(parameters.Q))
            {
                var searchTerm = parameters.Q.ToLowerInvariant();
                query = query.Where(x =>
                    EF.Functions.ILike(x.Name, $"%{searchTerm}%")
                    || EF.Functions.ILike(x.Slug, $"%{searchTerm}%")
                    || (x.Description != null && EF.Functions.ILike(x.Description, $"%{searchTerm}%")));
            }

            // Parse: Validate and parse querying parameters
            var parsing = parameters.ParseAll();
            if (parsing.IsFailure)
                return parsing.Errors;

            var pagedResult = await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToStoreSearch<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}
