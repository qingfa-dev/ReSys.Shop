using Module.Profile.Domain;
using Module.Profile.Domain.Notifications;

namespace Module.Profile.Features.Store.NotificationPreferences.Update;

public static partial class UpdateNotificationPreferences
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(currentUser.UserId))
                return UserProfileResult.Failure.NotFound;

            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == Guid.Parse(currentUser.UserId), cancellationToken);
            if (profile is null)
                return UserProfileResult.Failure.NotFound;

            var prefs = Module.Profile.Domain.Notifications.NotificationPreferences.Create(
                enableSms: command.Request.EnableSms,
                enableEmail: command.Request.EnableEmail,
                enableNewsfeeds: command.Request.EnableNewsfeeds);
            if (prefs.IsFailure)
                return prefs.Errors;

            profile.Notifications = prefs.Value;
            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response
            {
                EnableSms = profile.Notifications.EnableSms,
                EnableEmail = profile.Notifications.EnableEmail,
                EnableNewsfeeds = profile.Notifications.EnableNewsfeeds
            };
        }
    }
}
