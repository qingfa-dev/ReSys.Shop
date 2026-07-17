using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Storefront.Products.Shared.Mappings;

namespace Module.Catalog.Features.Storefront.Products.Get.List;

public static partial class ListProducts
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

            var parsing = parameters.ParseAll(
                allowedFilterFields: ProductConstant.Query.AllowedFilterFields,
                allowedSearchFields: ProductConstant.Query.AllowedSearchFields,
                allowedSortFields: ProductConstant.Query.AllowedSortFields);
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