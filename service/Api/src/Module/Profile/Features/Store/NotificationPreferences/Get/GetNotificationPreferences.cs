using Module.Profile.Domain;

namespace Module.Profile.Features.Store.NotificationPreferences.Get;

/// <summary>Retrieves the notification preferences for the authenticated user.</summary>
public static partial class GetNotificationPreferences
{
    public sealed record Query : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Loads the user profile and returns SMS, email, and newsfeed notification flags.</summary>
        /// <param name="request">The empty query.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the notification preferences or a not-found error.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=user authenticated, post=preferences returned or NotFound
            // Check: Ensure user is authenticated
            if (string.IsNullOrEmpty(currentUser.UserId))
                return UserProfileResult.Failure.NotFound;

            // Load: Fetch user profile to access notification settings
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == Guid.Parse(currentUser.UserId), cancellationToken);
            if (profile is null)
                return UserProfileResult.Failure.NotFound;

            // EXCEPTION: no domain entity — maps from domain NotificationPreferences values
            return new Response
            {
                EnableSms = profile.Notifications.EnableSms,
                EnableEmail = profile.Notifications.EnableEmail,
                EnableNewsfeeds = profile.Notifications.EnableNewsfeeds
            };
        }
    }
}