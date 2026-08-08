using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Storefront.Classifications.Shared.Models;
using Module.Catalog.Features.Storefront.Products.Shared;
using Module.Catalog.Features.Storefront.Products.Shared.Mappings;
using Module.Catalog.Features.Storefront.Products.Shared.Models;
using Module.Inventory.Services;

namespace Module.Catalog.Features.Storefront.Products.Get.List;

public static partial class GetStorefrontProducts
{
    #region Query

    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    #endregion

    #region Handler

    public sealed class PagedQueryHandler(
        IApplicationDbContext dbContext,
        IStockAvailabilityCalculator calculator)
        : IPagedQueryHandler<Query, Response>
    {
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            #region Query

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

            #endregion

            #region Filters

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

            #endregion

            #region Parsing

            // Resolve: Map storefront sort aliases (e.g. "Price") to EF Core paths
            var resolvedSort = parameters.Sort?
                .Select(s => StoreProductConstant.ResolveSortField(s.TrimStart('-', '+'))
                    is var field && s.StartsWith('-') ? $"-{field}" : s)
                .ToArray();

            var resolvedParams = parameters with { Sort = resolvedSort };

            var parsing = resolvedParams.ParseAll(
                allowedFilterFields: ProductConstant.Query.AllowedFilterFields,
                allowedSearchFields: ProductConstant.Query.AllowedSearchFields,
                allowedSortFields: StoreProductConstant.AllowedSortFields);
            if (parsing.IsFailure)
                return parsing.Errors;

            #endregion

            #region Paged Result

            var pagedResult = await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToStoreListItem<Response>(), cancellationToken);

            #endregion

            #region Stock

            var masterVariantIds = pagedResult.Items
                .Where(i => i.MasterVariant is not null)
                .Select(i => i.MasterVariant!.Id)
                .Distinct()
                .ToList();

            var availableByVariant = await calculator.GetAvailableByVariantAsync(masterVariantIds, cancellationToken);
            var backorderableByVariant = await calculator.GetBackorderableByVariantAsync(masterVariantIds, cancellationToken);

            #endregion

            #region Taxons

            var allTaxons = await dbContext.Set<Taxon>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            var taxonLookup = allTaxons.ToDictionary(t => t.Id, t => t);

            #endregion

            #region Enrichment

            var items = pagedResult.Items.Select(item =>
            {
                if (item.MasterVariant is not null)
                {
                    var available = availableByVariant.GetValueOrDefault(item.MasterVariant.Id, 0);
                    var backorderable = backorderableByVariant.GetValueOrDefault(item.MasterVariant.Id, false);
                    item = item with
                    {
                        MasterVariant = item.MasterVariant with
                        {
                            Stock = (available, backorderable).MapToStockInfo()
                        }
                    };
                }

                var taxonsWithBreadcrumbs = item.Classifications.Select<StoreTaxonListItemResponse, StoreTaxonListItemResponse>(t =>
                {
                    var taxonEntity = taxonLookup.GetValueOrDefault(t.Id);
                    var breadcrumb = new List<TaxonBreadcrumbItem>();
                    Taxon? current = taxonEntity;
                    while (current is not null)
                    {
                        breadcrumb.Insert(0, new TaxonBreadcrumbItem(current.Id, current.Name, current.Permalink));
                        current = current.ParentId is not null && taxonLookup.TryGetValue(current.ParentId.Value, out var parent)
                            ? parent
                            : null;
                    }

                    return t with { Breadcrumb = breadcrumb };
                }).ToList();

                return item with { Classifications = taxonsWithBreadcrumbs };
            }).ToList();

            #endregion

            return PagedResult<Response>.Create(items, pagedResult.PageNumber, pagedResult.PageSize, pagedResult.TotalCount);
        }
    }

    #endregion
}
