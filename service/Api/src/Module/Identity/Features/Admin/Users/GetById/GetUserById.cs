using Module.Identity.Features.Admin.Users.Shared.Mappings;

using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Users.GetById;

public static partial class GetUserById
{
    public record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Check: Attempt to find the user by its unique identifier.
            var user = await dbContext.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            if (user is null)
                return UserResult.Failure.NotFound;

            // Map: Convert the user entity to the detailed response DTO.
            var response = user.MapToDetail<Response>();

            return response;
        }
    }
}