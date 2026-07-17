using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Features.Admin.OptionTypes.Shared.Mappings;

namespace Module.Catalog.Features.Admin.OptionTypes.Get.Paged;

/// <summary>
/// Defines the use case for retrieving a paged or full list of option types.
/// </summary>
public static partial class GetOptionTypesPaged
{
    public record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>
        /// Retrieves a paged or full list of option types with filtering and sorting support.
        /// </summary>
        /// <param name="request">The query containing pagination and filtering parameters.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A paged result of option type list items.</returns>
        // Contract: pre=request.Parameters!=null, post=result.Items!=null
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            // Parse: Validate and parse querying parameters for pagination, filtering, and sorting
            var parseAll = parameters.ParseAll(
                allowedFilterFields: OptionTypeConstant.Query.AllowedFilterFields,
                allowedSearchFields: OptionTypeConstant.Query.AllowedSearchFields,
                allowedSortFields: OptionTypeConstant.Query.AllowedSortFields);
            if (parseAll.IsFailure)
                return parseAll.Errors;

            // Load: Retrieve option types, apply querying options, and map to paged result.
            var pagedResult = await dbContext.Set<OptionType>()
                .Include(x => x.OptionValues)
                .Include(x => x.ProductOptionTypes)
                .AsNoTracking()
                .ApplyQuerying(parseAll.Value)
                .ToPagedOrAllAsync(parseAll.Value, x => x.MapToListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}