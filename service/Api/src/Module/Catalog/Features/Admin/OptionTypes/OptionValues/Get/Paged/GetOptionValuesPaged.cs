using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Features.Admin.OptionTypes.OptionValues.Shared.Mappings;

namespace Module.Catalog.Features.Admin.OptionTypes.OptionValues.Get.Paged;

/// <summary>
/// Defines the use case for retrieving a paged or full list of option values.
/// </summary>
public static partial class GetOptionValuesPaged
{
    public record Query(Guid OptionTypeId, Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>
        /// Retrieves a paged or full list of option values for an option type with filtering and sorting.
        /// </summary>
        /// <param name="request">The query containing the option type ID, pagination, and filtering parameters.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A paged result of option value list items.</returns>
        // Contract: pre=request.OptionTypeId!=Guid.Empty, post=result.Items!=null
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Parse: Validate and parse querying parameters
            var parametersResult = request.Parameters.ParseAll();

            // Load: Retrieve option values, apply filtering/sorting, and project to paged result
            if (parametersResult.IsFailure)
            {
                // Apply paged query logic
                return parametersResult.Errors;
            }

            // Compute: Retrieve option values, apply filtering/sorting, and project to paged result
            var pagedResult = await dbContext.Set<OptionValue>()
                .Include(x => x.OptionType)
                .AsNoTracking()
                .Where(x => x.OptionTypeId == request.OptionTypeId)
                .ApplyQuerying(parametersResult.Value)
                .ToPagedOrAllAsync(parametersResult.Value, x => x.MapToListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}