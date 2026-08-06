using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Storefront.Products.Shared.Mappings;
using Module.Catalog.Features.Storefront.Products.Shared.Models;
using Module.Inventory.Services;

namespace Module.Catalog.Features.Storefront.Products.Get.Detail;

public static partial class GetProductDetail
{
    #region Query

    public sealed record Query(string Slug) : IQuery<Response>;

    public record Response : StoreProductDetailResponse;

    #endregion

    #region Handler

    public sealed class QueryHandler(
        IApplicationDbContext dbContext,
        ILogger<QueryHandler> logger,
        IStockAvailabilityCalculator calculator) : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            #region Query

            var entity = await dbContext.Set<Product>()
                .Include(x => x.Variants)
                    .ThenInclude(v => v.Prices)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.VariantImages)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.OptionValueVariants)
                        .ThenInclude(ov => ov.OptionValue)
                .Include(x => x.Classifications)
                    .ThenInclude(c => c.Taxon)
                .FirstOrDefaultAsync(x => x.Slug == query.Slug
                    && !x.IsDeleted
                    && x.AvailableOn <= DateTimeOffset.UtcNow, cancellationToken);

            #endregion

            if (entity is null)
            {
                ProductLoggers.StorefrontProductNotFoundBySlug(logger, query.Slug);
                return ProductResult.Errors.NotFoundBySlug(query.Slug);
            }

            ProductLoggers.StorefrontProductDetailLoaded(logger, query.Slug, entity.Id);

            var response = entity.MapToStoreDetail<Response>();

            #region Stock

            var variantIds = new List<Guid>();
            if (response.MasterVariant is not null)
                variantIds.Add(response.MasterVariant.Id);
            variantIds.AddRange(response.Variants.Select(v => v.Id));

            var availableByVariant = await calculator.GetAvailableByVariantAsync(variantIds, cancellationToken);
            var backorderableByVariant = await calculator.GetBackorderableByVariantAsync(variantIds, cancellationToken);

            if (response.MasterVariant is not null)
            {
                var available = availableByVariant.GetValueOrDefault(response.MasterVariant.Id, 0);
                var backorderable = backorderableByVariant.GetValueOrDefault(response.MasterVariant.Id, false);
                response = response with
                {
                    MasterVariant = response.MasterVariant with
                    {
                        Stock = (available, backorderable).MapToStockInfo()
                    }
                };
            }

            response = response with
            {
                Variants = response.Variants.Select(v =>
                {
                    var available = availableByVariant.GetValueOrDefault(v.Id, 0);
                    var backorderable = backorderableByVariant.GetValueOrDefault(v.Id, false);
                    return v with { Stock = (available, backorderable).MapToStockInfo() };
                }).ToList()
            };

            #endregion

            #region Taxons

            var taxons = await dbContext.Set<Taxon>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            var taxonLookup = taxons.ToDictionary(t => t.Id, t => t);

            for (int i = 0; i < response.Classifications.Count; i++)
            {
                var taxon = taxonLookup.GetValueOrDefault(response.Classifications[i].Id);
                if (taxon is null)
                    continue;

                var breadcrumb = new List<TaxonBreadcrumbItem>();
                Taxon? current = taxon;
                while (current is not null)
                {
                    breadcrumb.Insert(0, new TaxonBreadcrumbItem(current.Id, current.Name, current.Permalink));
                    current = current.ParentId is not null && taxonLookup.TryGetValue(current.ParentId.Value, out var parent)
                        ? parent
                        : null;
                }

                response.Classifications[i] = new StoreProductTaxonResponse
                {
                    Id = taxon.Id,
                    Name = taxon.Name,
                    Permalink = taxon.Permalink,
                    Depth = taxon.Depth,
                    Breadcrumb = breadcrumb
                };
            }

            #endregion

            return response;
        }
    }

    #endregion
}
