using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Storefront.Products.Shared.Mappings;

namespace Module.Catalog.Features.Storefront.Products.Get.List;

/// <summary>
/// Defines the use case for listing storefront products.
/// </summary>
public static partial class GetStorefrontProductPagedOrAll
{
    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>
        /// Retrieves a paged list of available storefront products with filtering, sorting, and search support.
        /// </summary>m
        /// <param name="request">The query containing pagination, filtering, and sorting parameters.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A paged result of storefront product list items.</returns>
        // Contract: pre=request.Parameters!=null, post=result.Items!=null
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            // Load: Available products with variants, prices, images, option values, and classifications
            var query = dbContext.Set<Product>()
                .Include(x => x.Variants)
                    .ThenInclude(v => v.Prices)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.VariantImages)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.OptionValueVariants)
                        .ThenInclude(ov => ov.OptionValue!)
                            .ThenInclude(o => o.OptionType!)
                .Include(x => x.Classifications)
                    .ThenInclude(c => c.Taxon)
                .Where(x => !x.IsDeleted && x.AvailableOn <= DateTimeOffset.UtcNow)
                .AsNoTracking();

            // Filter: Apply direct storefront filters by Id (arrays use OR semantics)
            if (parameters.OptionValueId is { Length: > 0 })
            {
                var optionValueIds = parameters.OptionValueId;
                query = query.Where(p => p.Variants.Any(v =>
                    v.OptionValueVariants.Any(ov =>
                        ov.OptionValue != null && optionValueIds.Contains(ov.OptionValue.Id))));
            }

            if (parameters.TaxonId is { Length: > 0 })
            {
                var taxonIds = parameters.TaxonId;
                query = query.Where(p => p.Classifications.Any(c =>
                    c.Taxon != null && taxonIds.Contains(c.Taxon.Id)));
            }

            if (parameters.MinPrice.HasValue)
            {
                var minPrice = parameters.MinPrice.Value;
                query = query.Where(p => p.Variants.Any(v =>
                    v.Prices.Any(pr => pr.Amount >= minPrice)));
            }

            if (parameters.MaxPrice.HasValue)
            {
                var maxPrice = parameters.MaxPrice.Value;
                query = query.Where(p => p.Variants.Any(v =>
                    v.Prices.Any(pr => pr.Amount <= maxPrice)));
            }

            // Parse: Validate and parse querying parameters for filtering, searching, and sorting
            var parsing = parameters.ParseAll(
                allowedFilterFields: ProductConstant.Query.AllowedFilterFields,
                allowedSearchFields: ProductConstant.Query.AllowedSearchFields,
                allowedSortFields: ProductConstant.Query.AllowedSortFields);
            if (parsing.IsFailure)
                return parsing.Errors;

            // Compute: Apply filtering, sorting, and pagination to produce the storefront result
            var pagedResult = await query
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToStoreResponse<Response>(), cancellationToken);
            return pagedResult;
        }
    }
}