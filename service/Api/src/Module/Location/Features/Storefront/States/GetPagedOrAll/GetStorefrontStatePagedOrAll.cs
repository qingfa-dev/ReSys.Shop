using Module.Location.Domain.States;
using Module.Location.Features.Shared.States.Mappings;

using Shared.Operational.Persistence.Specifications.Paging.Extensions;

namespace Module.Location.Features.Storefront.States.GetPagedOrAll;

/// <summary>Retrieves paged states for the storefront with filtering and sorting.</summary>
public static partial class GetStorefrontStatePagedOrAll
{
    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Loads and paginates states for storefront display.</summary>
        /// <param name="request">The query containing paging parameters.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A paged result of state list items.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=paged result returned
            var parameters = request.Parameters;

            var parsing = parameters.ParseAll(
                allowedFilterFields: StateConstant.Query.AllowedFilterFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSearchFields: StateConstant.Query.AllowedSearchFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSortFields: StateConstant.Query.AllowedSortFields.ToHashSet(StringComparer.OrdinalIgnoreCase));
            if (parsing.IsFailure)
                return PagedResult<Response>.Create(errors: parsing.Errors);

            // Load: Retrieve states with querying options.
            var pagedResult = await dbContext.Set<State>()
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(m => m.MapToListItem<Response>(), parsing.Value.Page, cancellationToken);

            // Map: Return paged result.
            return pagedResult;
        }
    }
}