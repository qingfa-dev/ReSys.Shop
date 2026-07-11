using Module.Profile.Domain;
using Module.Profile.Features.Store.Profile.Shared.Mappings;

using Shared.Operational.Persistence.Specifications.Paging.Extensions;

namespace Module.Profile.Features.Store.Profile.Get.PagedOrAll;

/// <summary>Retrieves all user profiles with pagination, filtering, and sorting.</summary>
public static partial class GetProfilesPagedOrAll
{
    /// <param name="Parameters">The querying parameters for pagination and filtering.</param>
    public record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Parses query parameters, loads profiles, and returns paginated results.</summary>
        /// <param name="request">The query with paging and filtering parameters.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A paged result containing profile list items.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            var parsing = parameters.ParseAll();
            if (parsing.IsFailure)
                return PagedResult<Response>.Create(errors: parsing.Errors);

            // Load: Retrieve all profiles from the database context.
            var profiles = dbContext.Set<UserProfile>();

            // Apply: Apply querying options (pagination, filtering, searching, ordering) and map to response DTOs.
            var pagedResult = await profiles
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(p => p.MapToListItem<Response>(), parsing.Value.Page, cancellationToken);

            return pagedResult;
        }
    }
}
