using Module.Profile.Domain;

namespace Module.Profile.Features.Store.NotificationPreferences.Get;

public static partial class GetNotificationPreferences
{
    public sealed record Query : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(currentUser.UserId))
                return UserProfileResult.Failure.NotFound;

            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == Guid.Parse(currentUser.UserId), cancellationToken);
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
