using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Features.Storefront.Options.Shared.Mappings;

namespace Module.Catalog.Features.Storefront.Options.Values;

/// <summary>
/// Defines the use case for retrieving all filterable option values.
/// </summary>
public static partial class GetStoreOptionValues
{
    public record Parameters : QueryingParameters;

    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>
        /// Retrieves all filterable option types with their ordered values for the storefront filter panel.
        /// </summary>
        /// <param name="request">The query containing pagination and filtering parameters.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A paged result of storefront option type responses.</returns>
        // Contract: pre=none, post=result.Items!=null
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            // Load: Filterable option types with ordered values for storefront display
            var query = dbContext.Set<OptionType>()
                .Include(x => x.OptionValues.OrderBy(v => v.Position))
                .Where(x => !x.IsDeleted && x.Filterable)
                .OrderBy(x => x.Position)
                .AsNoTracking();

            // Parse: Validate and parse querying parameters
            var parsing = parameters.ParseAll(
                allowedFilterFields: OptionTypeConstant.Query.AllowedFilterFields,
                allowedSearchFields: OptionTypeConstant.Query.AllowedSearchFields,
                allowedSortFields: OptionTypeConstant.Query.AllowedSortFields);
            if (parsing.IsFailure)
                return parsing.Errors;

            // Compute: Apply filtering, sorting, and pagination to produce the storefront result
            var pagedResult = await query
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToStoreResponse<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}