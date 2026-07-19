using Module.Profile.Domain;

namespace Module.Profile.Features.Store.NotificationPreferences.Get;

/// <summary>Retrieves the notification preferences for the authenticated user.</summary>
public static partial class GetNotificationPreferences
{
    public sealed record Query(Guid UserId) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);
            if (profile is null)
                return UserProfileResult.Failure.NotFound;

            return new Response
            {
                EnableSms = profile.Notifications.EnableSms,
                EnableEmail = profile.Notifications.EnableEmail,
                EnableNewsfeeds = profile.Notifications.EnableNewsfeeds
            };
        }
    }
}
