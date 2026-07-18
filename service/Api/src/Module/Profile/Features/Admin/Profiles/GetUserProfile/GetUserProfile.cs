using Module.Profile.Domain;
using Module.Profile.Features.Admin.Profiles.Shared.Mappings;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Admin.Profiles.GetUserProfile;

public static partial class GetUserProfile
{
    public sealed record Query(Guid UserId) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = await dbContext.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user is null)
                return UserProfileResult.Failure.UserNotFound;

            var profile = await dbContext.Set<UserProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

            if (profile is null)
                return UserProfileResult.Failure.NotFound;

            return profile.MapToDetail<Response>(user);
        }
    }
}
