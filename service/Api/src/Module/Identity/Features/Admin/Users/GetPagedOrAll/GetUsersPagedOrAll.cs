using Module.Identity.Features.Admin.Users.Shared.Mappings;

using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Users.GetPagedOrAll;

public static partial class GetUsersPagedOrAll
{
    public record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            var parsing = parameters.ParseAll();
            if (parsing.IsFailure)
                return parsing.Errors;

            // Get: Retrieve all users from the database context.
            var users = dbContext.Set<User>();

            // Apply: Apply querying options (pagination, filtering, searching, ordering) and map to response DTOs.
            var pagedResult = await users
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, u => u.MapToListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}