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
        /// Handles the request and returns a result.
        /// </summary>
        /// <param name="request">The query containing request data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        // Contract: pre=request!=null, post=result!=null
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            // Parse: Validate and parse querying parameters for pagination, filtering, and sorting
            var parseAll = parameters.ParseAll();
            if (parseAll.IsFailure)
                return parseAll.Errors;

            // Query: Retrieve option types, apply querying options, and map to paged result.
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
