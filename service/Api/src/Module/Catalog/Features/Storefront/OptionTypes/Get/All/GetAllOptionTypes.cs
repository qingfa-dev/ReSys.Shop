using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Features.Storefront.OptionTypes.Shared.Mappings;

namespace Module.Catalog.Features.Storefront.OptionTypes.Get.All;

/// <summary>
/// Defines the use case for retrieving all filterable option types.
/// </summary>
public static partial class GetAllOptionTypes
{
    public record Parameters : QueryingParameters;

    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <inheritdoc />
        // Contract: pre=none, post=result.Items!=null
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            var query = dbContext.Set<OptionType>()
                .Include(x => x.OptionValues.OrderBy(v => v.Position))
                .Where(x => !x.IsDeleted && x.Filterable)
                .OrderBy(x => x.Position)
                .AsNoTracking();

            // Parse: Validate and parse querying parameters
            var parsing = parameters.ParseAll();
            if (parsing.IsFailure)
                return parsing.Errors;

            var pagedResult = await query
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToStoreResponse<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}