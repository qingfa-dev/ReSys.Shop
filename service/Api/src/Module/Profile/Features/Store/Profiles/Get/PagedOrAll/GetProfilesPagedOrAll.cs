using Module.Profile.Domain;
using Module.Profile.Features.Store.Profile.Shared.Mappings;

using Shared.Operational.Persistence.Specifications.Paging.Extensions;
using Shared.Operational.Persistence.Specifications.Querying;

namespace Module.Profile.Features.Store.Profile.Get.PagedOrAll;

/// <summary>
/// Defines the use case for retrieving all user profiles with pagination and filtering.
/// </summary>
public static partial class GetProfilesPagedOrAll
{
    /// <summary>
    /// Represents the query to retrieve profiles with optional pagination.
    /// </summary>
    /// <param name="Parameters">The querying parameters for pagination and filtering.</param>
    public record Query(Parameters Parameters) : IPagedQuery<Response>;

    /// <summary>
    /// Handles the <see cref="Query"/> to retrieve paginated or all profiles.
    /// </summary>
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>
        /// Handles retrieval of profiles with pagination, filtering, and sorting.
        /// </summary>
        /// <param name="request">The query with parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A paged result containing profile list items.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            var parsing = parameters.ParseAll();
            if (parsing.IsFailure)
                return PagedResult<Response>.Create(errors: parsing.Errors);

            // Get: Retrieve all profiles from the database context.
            var profiles = dbContext.Set<UserProfile>();

            // Apply: Apply querying options (pagination, filtering, searching, ordering) and map to response DTOs.
            var pagedResult = await profiles
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(p => p.MapToListItem<Response>(), parsing.Value.Page, cancellationToken);

            return pagedResult;
        }
    }
}
