using Module.Profile.Domain;
using Module.Profile.Features.Store.Profiles.Shared.Mappings;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Profiles.Get.Detail;

/// <summary>Retrieves the full profile detail for the authenticated user.</summary>
public static partial class GetProfile
{
    public sealed record Query(Guid UserId) : IQuery<Response>;

    public sealed class QueryHandler(
        ICurrentUser currentUser,
        IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Validates user identity, loads the user and profile, and returns a combined detail response.</summary>
        /// <param name="request">The query containing the user ID.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the full profile combined with user data or an error.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=user authenticated && request.UserId==currentUser, post=profile detail or NotFound
            // Check: Ensure user is authenticated
            if (!currentUser.IsAuthenticated || string.IsNullOrEmpty(currentUser.UserId))
                return UserProfileResult.Failure.AuthRequired;

            if (!Guid.TryParse(currentUser.UserId, out var currentUserId))
                return UserProfileResult.Failure.AuthRequired;

            // Check: Ensure user can only access their own profile
            if (request.UserId != currentUserId)
                return UserProfileResult.Failure.AccessDenied;

            // Load: Fetch identity user
            var user = await dbContext.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (user is null)
                return UserProfileResult.Failure.UserNotFound;

            // Load: Fetch user profile
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(pu => pu.UserId == request.UserId, cancellationToken);

            if (profile is null)
                return UserProfileResult.Failure.NotFound;

            // Map: Return combined user and profile response
            return profile.MapToDetail<Response>(user);
        }
    }
}