using Module.Catalog.Domain.Variants.Prices;
using Module.Catalog.Features.Admin.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Variants.Prices.Get;

/// <summary>
/// Defines the use case for listing prices by variant.
/// </summary>
public static partial class ListPricesByVariant
{
    public record Parameters : QueryingParameters
    {
        // Additional filtering parameters can be added here if needed
        public string? Currency { get; init; }
        public Guid? VariantId { get; init; }
    }
    public sealed record Query(Guid VariantId, Parameters Parameters) : IPagedQuery<Response>;

    /// <summary>
    /// Lists prices for a variant with pagination support.
    /// </summary>
    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>
        /// Handles the list-prices query — builds a filtered query by variant ID,
        /// applies pagination options, and returns paged or all results.
        /// </summary>
        /// <param name="request">The query containing the variant ID and pagination parameters.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A paged result of price items.</returns>
        // Contract: pre=query.VariantId!=Guid.Empty, post=result.Items!=null
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var (variantId, parameters) = request;


            // Load: Base query for prices filtered by variant
            var query = dbContext.Set<Price>()
                .Where(p => p.VariantId == variantId && p.DeletedAt == null)
                .AsNoTracking();

            // Paginate: Apply query options and return paged or all results
            // Parse: Validate and parse querying parameters
            var parsing = parameters.ParseAll(
                allowedFilterFields: PriceConstant.Query.AllowedFilterFields,
                allowedSearchFields: PriceConstant.Query.AllowedSearchFields,
                allowedSortFields: PriceConstant.Query.AllowedSortFields);
            if (parsing.IsFailure)
                return parsing.Errors;

            var pagedResult = await query
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToDetail<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}