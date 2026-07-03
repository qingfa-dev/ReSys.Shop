using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Storefront.Products.Shared.Mappings;

namespace Module.Catalog.Features.Storefront.Products.Get.Filter;

/// <summary>
/// Defines the use case for filtering products by attributes.
/// </summary>
public static partial class FilterProducts
{
    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>
        /// Handles faceted product filtering by attributes (color, size, price, material).
        /// Builds a filtered query using option values, prices, and properties
        /// then returns a paged result of product list items.
        /// </summary>
        /// <param name="request">The query containing filter and pagination parameters.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A paged result of filtered product list items.</returns>
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
                .Where(x => !x.IsDeleted && x.AvailableOn <= DateTimeOffset.UtcNow)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(parameters.Color))
            {
                query = query.Where(x => x.Variants
                    .Any(v => v.OptionValueVariants
                        .Any(ov => ov.OptionValue != null
                            && ov.OptionValue.OptionType.Name == "Color"
                            && EF.Functions.ILike(ov.OptionValue.Name, parameters.Color))));
            }

            if (!string.IsNullOrWhiteSpace(parameters.Size))
            {
                query = query.Where(x => x.Variants
                    .Any(v => v.OptionValueVariants
                        .Any(ov => ov.OptionValue != null
                            && ov.OptionValue.OptionType.Name == "Size"
                            && EF.Functions.ILike(ov.OptionValue.Name, parameters.Size))));
            }

            if (parameters.MinPrice.HasValue)
            {
                query = query.Where(x => x.Variants
                    .Any(v => v.Prices.Any(p => p.Amount >= parameters.MinPrice.Value)));
            }

            if (parameters.MaxPrice.HasValue)
            {
                query = query.Where(x => x.Variants
                    .Any(v => v.Prices.Any(p => p.Amount <= parameters.MaxPrice.Value)));
            }

            if (!string.IsNullOrWhiteSpace(parameters.Material))
            {
                query = query.Where(x => x.Variants
                    .Any(v => v.OptionValueVariants
                        .Any(ov => ov.OptionValue != null
                            && ov.OptionValue.OptionType.Name == "Material"
                            && EF.Functions.ILike(ov.OptionValue.Name, parameters.Material))));
            }

            // Parse: Validate and parse querying parameters
            var parsing = parameters.ParseAll();
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
