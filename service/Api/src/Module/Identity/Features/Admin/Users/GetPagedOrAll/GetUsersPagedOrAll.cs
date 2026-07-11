using Module.Identity.Features.Admin.Users.Shared.Mappings;

using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Users.GetPagedOrAll;

public static partial class GetUsersPagedOrAll
{
    public record Query(Parameters Parameters) : IPagedQuery<Response>;

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

            var parsing = parameters.ParseAll();
            if (parsing.IsFailure)
                return parsing.Errors;

            var users = dbContext.Set<User>();

            var pagedResult = await users
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, u => u.MapToListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}