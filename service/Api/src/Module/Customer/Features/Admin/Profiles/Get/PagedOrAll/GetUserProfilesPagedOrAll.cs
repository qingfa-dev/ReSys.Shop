using Module.Customer.Domain;
using Module.Customer.Features.Shared.Profiles.Mappings;

using Shared.Operational.Persistence.Specifications.Paging.Extensions;

namespace Module.Customer.Features.Admin.Profiles.Get.PagedOrAll;

/// <summary>Retrieves all user profiles with pagination, filtering, and sorting.</summary>
public static partial class GetUserProfilesPagedOrAll
{
    /// <param name="Parameters">The querying parameters for pagination and filtering.</param>
    public record Query(Parameters Parameters) : IPagedQuery<Response>;

    /// <summary>Handles paginated retrieval of user profiles with filtering and sorting.</summary>
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

            var parsing = parameters.ParseAll(
                allowedFilterFields: UserProfileConstant.Query.AllowedFilterFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSearchFields: UserProfileConstant.Query.AllowedSearchFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSortFields: UserProfileConstant.Query.AllowedSortFields.ToHashSet(StringComparer.OrdinalIgnoreCase));
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