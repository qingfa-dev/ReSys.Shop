using Module.Profile.Domain;
using Module.Profile.Features.Store.Profile.Shared.Mappings;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Profile.Get.Detail;

public static partial class GetProfile
{
    public sealed record Query(Guid UserId) : IQuery<Response>;

    public sealed class QueryHandler(
        ICurrentUser currentUser,
        IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (!currentUser.IsAuthenticated || string.IsNullOrEmpty(currentUser.UserId))
                return UserProfileResult.Failure.AuthRequired;

            var user = await dbContext.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (user is null)
                return UserProfileResult.Failure.UserNotFound;

            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(pu => pu.UserId == request.UserId, cancellationToken);

            if (profile is null)
                return UserProfileResult.Failure.NotFound;

            return profile.MapToDetail<Response>(user);
        }
    }
}
