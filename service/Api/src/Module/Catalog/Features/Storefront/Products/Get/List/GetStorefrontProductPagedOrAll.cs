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
        /// </summary>
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

            // Compute: Facet counts against the filtered product set (option values + taxons).
            FacetAggregate? facets = null;
            if (parameters.IncludeFacets)
            {
                var productIds = await query
                    .Select(p => p.Id)
                    .ToListAsync(cancellationToken);

                // Compute: Count products per option value (facet group = option type).
                var optionValueCounts = await dbContext.Set<Product>()
                    .Where(p => productIds.Contains(p.Id))
                    .SelectMany(p => p.Variants)
                    .SelectMany(v => v.OptionValueVariants)
                    .Where(ov => ov.OptionValue != null && ov.OptionValue.OptionType != null)
                    .GroupBy(ov => new
                    {
                        ov.OptionValue!.OptionTypeId,
                        OptionTypeName = ov.OptionValue.OptionType.Name,
                        OptionTypePosition = ov.OptionValue.OptionType.Position,
                        OptionValueId = ov.OptionValue.Id,
                        OptionValueName = ov.OptionValue.Name,
                        OptionValuePosition = ov.OptionValue.Position
                    })
                    .Select(g => new
                    {
                        g.Key.OptionTypeId,
                        g.Key.OptionTypeName,
                        g.Key.OptionTypePosition,
                        g.Key.OptionValueId,
                        g.Key.OptionValueName,
                        g.Key.OptionValuePosition,
                        Count = g.Select(ov => ov.Variant!.ProductId).Distinct().Count()
                    })
                    .ToListAsync(cancellationToken);

                var optionValueGroups = optionValueCounts
                    .GroupBy(c => new { c.OptionTypeId, c.OptionTypeName, c.OptionTypePosition })
                    .Select(g => new FacetGroup(
                        g.Key.OptionTypeName,
                        g
                            .OrderBy(c => c.OptionValuePosition)
                            .Select(c => new FacetValue(
                                c.OptionValueId.ToString(),
                                c.OptionValueName,
                                c.Count,
                                parameters.OptionValueId?.Contains(c.OptionValueId) == true))
                            .ToList()))
                    .OrderBy(g => g.Values.FirstOrDefault()?.Id)
                    .ToList();

                // Compute: Count products per taxon.
                var taxonCounts = await dbContext.Set<Product>()
                    .Where(p => productIds.Contains(p.Id))
                    .SelectMany(p => p.Classifications)
                    .Where(c => c.Taxon != null)
                    .GroupBy(c => new
                    {
                        c.Taxon!.Id,
                        c.Taxon.Name,
                        c.Taxon.Position
                    })
                    .Select(g => new
                    {
                        g.Key.Id,
                        g.Key.Name,
                        g.Key.Position,
                        Count = g.Count()
                    })
                    .ToListAsync(cancellationToken);

                if (taxonCounts.Count != 0)
                {
                    optionValueGroups.Add(new FacetGroup(
                        "Category",
                        taxonCounts
                            .OrderBy(c => c.Position)
                            .Select(c => new FacetValue(
                                c.Id.ToString(),
                                c.Name,
                                c.Count,
                                parameters.TaxonId?.Contains(c.Id) == true))
                            .ToList()));
                }

                facets = new FacetAggregate(optionValueGroups);
            }

            // Compute: Order by newest, apply pagination, and project to storefront list items
            var pagedResult = await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToStoreListItem<Response>(), cancellationToken);

            if (facets is not null)
            {
                var items = pagedResult.Items
                    .Select(item => (Response)item with { Facets = facets })
                    .ToList();
                return PagedResult<Response>.Create(items, pagedResult.PageNumber, pagedResult.PageSize, pagedResult.TotalCount);
            }

            return pagedResult;
        }
    }
}