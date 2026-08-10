using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Taxons;
using Module.Catalog.Features.Storefront.Products.Shared;
using Module.Catalog.Features.Storefront.Products.Shared.Mappings;
using Module.Catalog.Features.Storefront.Products.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Get.ByTaxonPermalink;

/// <summary>Retrieves a paged list of products classified under a taxon resolved by permalink.</summary>
public static partial class GetProductsByTaxonPermalink
{
    public sealed record Query(string Permalink, Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Resolves taxon by permalink then returns paged products under that taxon.</summary>
        public async Task<PagedResult<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var taxon = await dbContext.Set<Taxon>()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Permalink == query.Permalink, cancellationToken);
            if (taxon is null)
                return PagedResult<Response>.NotFound();

            var parameters = query.Parameters;

            var parsing = parameters.ParseAll(
                allowedFilterFields: ProductConstant.Query.AllowedFilterFields,
                allowedSearchFields: ProductConstant.Query.AllowedSearchFields,
                allowedSortFields: StoreProductConstant.AllowedSortFields);
            if (parsing.IsFailure)
                return parsing.Errors;

            var pagedResult = await dbContext.Set<Product>()
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
                .Where(x => !x.IsDeleted
                            && x.AvailableOn <= DateTimeOffset.UtcNow
                            && x.Classifications.Any(c => c.Taxon != null && c.TaxonId == taxon.Id))
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAtUtc)
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToStoreListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}