using Module.Profile.Domain;

namespace Module.Profile.Features.Storefront.NotificationPreferences.Get;

/// <summary>Retrieves the notification preferences for the authenticated user.</summary>
public static partial class GetNotificationPreferences
{
    public sealed record Query(Guid UserId) : IQuery<Response>;

    /// <summary>Handles the retrieval of notification preferences for the current user.</summary>
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Retrieves notification preferences for the current user.</summary>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Load: Fetch the user's profile from persistence
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);
            // Validate: Confirm profile exists
            if (profile is null)
                return UserProfileResult.Failure.NotFound;

            // Transform: Extract notification preferences into response DTO
            return new Response
            {
                EnableSms = profile.Notifications.EnableSms,
                EnableEmail = profile.Notifications.EnableEmail,
                EnableNewsfeeds = profile.Notifications.EnableNewsfeeds
            };
        }
    }
}
