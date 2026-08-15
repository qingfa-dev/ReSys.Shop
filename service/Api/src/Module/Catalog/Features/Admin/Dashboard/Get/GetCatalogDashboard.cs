using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Variants;
using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxons;
using Module.Catalog.Features.Admin.Dashboard.Get.Shared.Models;

namespace Module.Catalog.Features.Admin.Dashboard.Get;

public static partial class GetCatalogDashboard
{
    /// <summary>Handler for getting the catalog dashboard data.</summary>
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Gets the catalog dashboard data.</summary>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Load: Base queries for non-deleted products, variants, taxonomies, and taxons
            var productsQuery = dbContext.Set<Product>().Where(p => !p.IsDeleted);
            var variantsQuery = dbContext.Set<Variant>().Where(v => !v.IsDeleted);
            var taxonomiesQuery = dbContext.Set<Taxonomy>().Where(t => !t.IsDeleted);
            var taxonsQuery = dbContext.Set<Taxon>().Where(t => !t.IsDeleted);

            // Aggregate: Compute dashboard counters from filtered queries
            var totalProducts = await productsQuery.CountAsync(cancellationToken);
            var activeProducts = await productsQuery.CountAsync(p => p.Status == ProductStatus.Active, cancellationToken);
            var draftProducts = await productsQuery.CountAsync(p => p.Status == ProductStatus.Draft, cancellationToken);
            var totalVariants = await variantsQuery.CountAsync(cancellationToken);
            var totalTaxonomies = await taxonomiesQuery.CountAsync(cancellationToken);
            var totalTaxons = await taxonsQuery.CountAsync(cancellationToken);

            // Load: Fetch the 5 most recently created products for the dashboard feed
            var recentProducts = await productsQuery
                .OrderByDescending(p => p.CreatedAtUtc)
                .Take(5)
                .Select(p => new RecentProductData { Id = p.Id, Name = p.Name, Slug = p.Slug, CreatedAtUtc = p.CreatedAtUtc })
                .ToListAsync(cancellationToken);

            return new Response
            {
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                DraftProducts = draftProducts,
                TotalVariants = totalVariants,
                TotalTaxonomies = totalTaxonomies,
                TotalTaxons = totalTaxons,
                RecentProducts = recentProducts
            };
        }
    }
}
