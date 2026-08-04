using Module.Profile.Domain;
using Module.Profile.Features.Shared.Profiles.Mappings;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Storefront.Profiles.Get.Detail;

/// <summary>Retrieves the current user's profile.</summary>
public static partial class GetProfile
{
    public sealed record Query(Guid UserId) : IQuery<Response>;

    /// <summary>Handles the retrieval of the current user's profile.</summary>
    public sealed class QueryHandler(
        IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Retrieves the current user's profile.</summary>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Load: Fetch the identity user from persistence
            var user = await dbContext.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            // Validate: Confirm identity user exists
            if (user is null)
                return UserProfileResult.Failure.UserNotFound;

            // Load: Fetch the user's profile from persistence
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(pu => pu.UserId == request.UserId, cancellationToken);

            // Validate: Confirm profile exists
            if (profile is null)
                return UserProfileResult.Failure.NotFound;

            // Transform: Map profile with user data to detail response DTO
            return profile.MapToDetail<Response>(user);
        }
    }
}
