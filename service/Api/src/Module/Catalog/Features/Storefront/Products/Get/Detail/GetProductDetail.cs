using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Storefront.Products.Shared.Mappings;
using Module.Catalog.Features.Storefront.Products.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Get.Detail;

/// <summary>
/// Defines the use case for retrieving product detail by slug.
/// </summary>
public static partial class GetProductDetail
{
    public sealed record Query(string Slug) : IQuery<Response>;

    public record Response : StoreProductDetailResponse;

    public sealed class QueryHandler(
        IApplicationDbContext dbContext,
        ILogger<QueryHandler> logger) : IQueryHandler<Query, Response>
    {
        /// <summary>Retrieves a product with full detail by slug — includes variants, prices, images, and classifications.</summary>
        // Contract: pre=query.Slug!=null, post=result!=null
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
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

            if (entity is null)
            {
                // Log: Record product not found for observability
                ProductLoggers.StorefrontProductNotFoundBySlug(logger, query.Slug);
                return ProductResult.Errors.NotFoundBySlug(query.Slug);
            }

            // Log: Record product detail loaded for observability
            ProductLoggers.StorefrontProductDetailLoaded(logger, query.Slug, entity.Id);

            var response = entity.MapToStoreDetail<Response>();

            // Compute: Populate breadcrumb trail (root → leaf) for each taxon classification.
            var taxons = await dbContext.Set<Module.Catalog.Domain.Taxonomies.Taxons.Taxon>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            var taxonLookup = taxons.ToDictionary(t => t.Id, t => t);

            for (int i = 0; i < response.Taxons.Count; i++)
            {
                var taxon = taxonLookup.GetValueOrDefault(response.Taxons[i].Id);
                if (taxon is null)
                    continue;

                var breadcrumb = new List<TaxonBreadcrumbItem>();
                Module.Catalog.Domain.Taxonomies.Taxons.Taxon? current = taxon;
                while (current is not null)
                {
                    breadcrumb.Insert(0, new TaxonBreadcrumbItem(current.Id, current.Name, current.Permalink));
                    current = current.ParentId is not null && taxonLookup.TryGetValue(current.ParentId.Value, out var parent)
                        ? parent
                        : null;
                }

                response.Taxons[i] = new StoreProductTaxonResponse
                {
                    Id = taxon.Id,
                    Name = taxon.Name,
                    Permalink = taxon.Permalink,
                    Depth = taxon.Depth,
                    Breadcrumb = breadcrumb
                };
            }

            return response;
        }
    }
}