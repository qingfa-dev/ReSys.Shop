using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Features.Storefront.Classifications.Shared.Mappings;
using Module.Catalog.Features.Storefront.Classifications.Shared.Models;

namespace Module.Catalog.Features.Storefront.Classifications.Taxons.GetByPermalink;

/// <summary>Retrieves a single taxon by its permalink with breadcrumb and direct children.</summary>
public static partial class GetTaxonByPermalink
{
    public sealed record Query(string Permalink) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext) : IQueryHandler<Query, Response>
    {
        /// <summary>Resolves a taxon by permalink and returns it with breadcrumb and direct children.</summary>
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var taxon = await dbContext.Set<Taxon>()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Permalink == query.Permalink, cancellationToken);
            if (taxon is null)
                return TaxonResult.Errors.NotFound;

            var taxons = await dbContext.Set<Taxon>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            var taxonLookup = taxons.ToDictionary(t => t.Id, t => t);

            var breadcrumb = new List<TaxonBreadcrumbItem>();
            Taxon? current = taxon;
            while (current is not null)
            {
                breadcrumb.Insert(0, new TaxonBreadcrumbItem(current.Id, current.Name, current.Permalink));
                current = current.ParentId is not null && taxonLookup.TryGetValue(current.ParentId.Value, out var parent)
                    ? parent
                    : null;
            }

            var children = taxons
                .Where(t => t.ParentId == taxon.Id)
                .OrderBy(t => t.Lft)
                .Select(t => new TaxonBreadcrumbItem(t.Id, t.Name, t.Permalink))
                .ToList();

            return taxon.MapToStoreListItem<Response>() with
            {
                Breadcrumb = breadcrumb,
                Children = children
            };
        }
    }
}