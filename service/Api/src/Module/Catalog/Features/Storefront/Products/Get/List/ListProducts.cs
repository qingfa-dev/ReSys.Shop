using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Storefront.Products.Shared.Mappings;

namespace Module.Catalog.Features.Storefront.Products.Get.List;

public static partial class ListProducts
{
    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

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

            foreach (IStorefrontProductAlias alias in StorefrontProductFilterAliases.All)
            {
                var predicate = alias.BuildPredicate(parameters);
                if (predicate is not null)
                    query = query.Where(predicate);
            }

            var allowedSearchFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Name", "Slug", "Description" };
            var allowedSortFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Name", "Slug", "AvailableOn", "CreatedAtUtc", "Variants.Prices.Amount"
            };

            var parsing = parameters.ParseAll(
                StorefrontProductFilterAliases.CanonicalFields,
                allowedSearchFields,
                allowedSortFields);
            if (parsing.IsFailure)
                return parsing.Errors;

            var pagedResult = await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToStoreListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}
