using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Shared.Application.Mediators.Queries;
using Shared.Operational.Persistence.Data;

namespace Module.Catalog.Features.Admin.Dashboard.Get;

public static partial class GetCatalogDashboard
{
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var productsQuery = dbContext.Set<Product>().Where(p => !p.IsDeleted);
            var variantsQuery = dbContext.Set<Variant>().Where(v => !v.IsDeleted);
            var taxonomiesQuery = dbContext.Set<Taxonomy>().Where(t => !t.IsDeleted);
            var taxonsQuery = dbContext.Set<Taxon>().Where(t => !t.IsDeleted);

            var totalProducts = await productsQuery.CountAsync(cancellationToken);
            var activeProducts = await productsQuery.CountAsync(p => p.Status == ProductStatus.Active, cancellationToken);
            var draftProducts = await productsQuery.CountAsync(p => p.Status == ProductStatus.Draft, cancellationToken);
            var totalVariants = await variantsQuery.CountAsync(cancellationToken);
            var totalTaxonomies = await taxonomiesQuery.CountAsync(cancellationToken);
            var totalTaxons = await taxonsQuery.CountAsync(cancellationToken);

            var recentProducts = await productsQuery
                .OrderByDescending(p => p.CreatedAtUtc)
                .Take(5)
                .Select(p => new RecentProductData(p.Id, p.Name, p.Slug, p.CreatedAtUtc.DateTime))
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
