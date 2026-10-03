using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Taxons;
using Module.Catalog.Features.Storefront.Shared.Models;
using Module.Catalog.Features.Storefront.Shared.Mappings;
using Module.Inventory.Services;

namespace Module.Catalog.Features.Storefront.Products.Get.Detail;

public static partial class GetProductDetail
{
    #region Query

    public sealed record Query(Guid Id) : IQuery<Response>;

    #endregion

    #region Handler

    public sealed class QueryHandler(
        IApplicationDbContext dbContext,
        ILogger<QueryHandler> logger,
        IStockItemService stockItem) : IQueryHandler<Query, Response>
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
                .FirstOrDefaultAsync(x => x.Id == query.Id
                    && !x.IsDeleted
                    && x.AvailableOn <= DateTimeOffset.UtcNow, cancellationToken);

            #endregion

            if (entity is null)
            {
                ProductLoggers.StorefrontProductNotFoundById(logger, query.Id);
                return ProductResult.Errors.NotFoundById(query.Id);
            }

            ProductLoggers.StorefrontProductDetailLoaded(logger, entity.Slug, entity.Id);

            var response = entity.MapToStoreDetail<Response>();

            #region Stock

            var variantIds = new List<Guid>();
            if (response.MasterVariant is not null)
                variantIds.Add(response.MasterVariant.Id);
            variantIds.AddRange(response.Variants.Select(v => v.Id));

            var availabilityResult = await stockItem.GetStockAvailabilityAsync(variantIds, cancellationToken);
            var availabilityMap = availabilityResult.Value.ToDictionary(a => a.VariantId);

            if (response.MasterVariant is not null)
            {
                var entry = availabilityMap.GetValueOrDefault(response.MasterVariant.Id);
                response = response with
                {
                    MasterVariant = response.MasterVariant with
                    {
                        Stock = entry?.MapToStockInfo() ?? new()
                    }
                };
            }

            response = response with
            {
                Variants = response.Variants.Select(v =>
                {
                    var entry = availabilityMap.GetValueOrDefault(v.Id);
                    return v with { Stock = entry?.MapToStockInfo() ?? new() };
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

                response.Classifications[i] = response.Classifications[i] with
                {
                    Breadcrumb = breadcrumb
                };
            }

            #endregion

            return response;
        }
    }

    #endregion
}
