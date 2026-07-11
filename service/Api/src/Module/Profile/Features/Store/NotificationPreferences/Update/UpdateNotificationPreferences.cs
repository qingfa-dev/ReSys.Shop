using Module.Profile.Domain;

namespace Module.Profile.Features.Store.NotificationPreferences.Update;

/// <summary>Updates the notification preferences for the authenticated user.</summary>
public static partial class UpdateNotificationPreferences
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Validates user, creates new preferences, and persists the change.</summary>
        /// <param name="command">The command containing SMS, email, and newsfeed toggle flags.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the updated preferences or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=user authenticated && profile exists, post=preferences updated, throws=DbUpdateException
            // Check: Ensure user is authenticated
            if (string.IsNullOrEmpty(currentUser.UserId))
                return UserProfileResult.Failure.NotFound;

            // Load: Fetch user profile
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == Guid.Parse(currentUser.UserId), cancellationToken);
            if (profile is null)
                return UserProfileResult.Failure.NotFound;

            // Create: Build notification preferences from request
            var prefs = Module.Profile.Domain.Notifications.NotificationPreferences.Create(
                enableSms: command.Request.EnableSms,
                enableEmail: command.Request.EnableEmail,
                enableNewsfeeds: command.Request.EnableNewsfeeds);
            if (prefs.IsFailure)
                return prefs.Errors;

            // Update: Replace profile notification preferences
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
