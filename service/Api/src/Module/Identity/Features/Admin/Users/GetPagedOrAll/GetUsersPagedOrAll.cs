using DomainUsers = Module.Identity.Domain.Users;
using Module.Identity.Features.Admin.Shared.Mappings;

using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Shared.Admin.Users.GetPagedOrAll;

public static partial class GetUsersPagedOrAll
{
    public record Query(Parameters Parameters) : IPagedQuery<Response>;

    /// <summary>
    /// Handles the <see cref="Query"/> to retrieve users with paging or all results.
    /// </summary>
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        // Contract: pre=request!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Retrieves users with optional paging, filtering, searching, and ordering applied.
        /// </summary>
        /// <param name="request">The query containing pagination and filtering parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A paged result containing user list items.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database query fails.</exception>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            // Validate: Parse and validate filter, search, and sort parameters against allowed fields
            var parsing = parameters.ParseAll(
                allowedFilterFields: DomainUsers.UserConstant.Query.AllowedFilterFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSearchFields: DomainUsers.UserConstant.Query.AllowedSearchFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSortFields: DomainUsers.UserConstant.Query.AllowedSortFields.ToHashSet(StringComparer.OrdinalIgnoreCase));
            if (parsing.IsFailure)
                return parsing.Errors;

            // Load: Access user queryable from the database context
            var users = dbContext.Set<User>();

            // Transform: Apply dynamic querying and projection, then paginate the results
            var pagedResult = await users
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, u => u.MapToListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}