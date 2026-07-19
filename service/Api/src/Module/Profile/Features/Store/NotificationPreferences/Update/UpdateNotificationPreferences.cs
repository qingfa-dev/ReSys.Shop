using Module.Profile.Domain;

namespace Module.Profile.Features.Store.NotificationPreferences.Update;

/// <summary>Updates the notification preferences for the authenticated user.</summary>
public static partial class UpdateNotificationPreferences
{
    public sealed record Command(Guid UserId, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == command.UserId, cancellationToken);
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
