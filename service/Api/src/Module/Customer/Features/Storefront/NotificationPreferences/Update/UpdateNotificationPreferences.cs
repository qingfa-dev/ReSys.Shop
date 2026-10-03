using Module.Customer.Domain;

namespace Module.Customer.Features.Storefront.NotificationPreferences.Update;

/// <summary>Updates the notification preferences for the authenticated user.</summary>
public static partial class UpdateNotificationPreferences
{
    public sealed record Command(Guid UserId, Request Request) : ICommand<Response>;

    /// <summary>Handles the update of notification preferences.</summary>
    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Updates notification preferences.</summary>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Load: Fetch the user's profile from persistence
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == command.UserId, cancellationToken);
            // Validate: Confirm profile exists
            if (profile is null)
                return UserProfileResult.Failure.NotFound;

            // Validate: Build and validate notification preferences value object
            var prefs = Module.Customer.Domain.Notifications.NotificationPreferences.Create(
                enableSms: command.Request.EnableSms,
                enableEmail: command.Request.EnableEmail,
                enableNewsfeeds: command.Request.EnableNewsfeeds);
            if (prefs.IsFailure)
                return prefs.Errors;

            // Update: Replace profile notification preferences
            profile.Notifications = prefs.Value;
            // Call: Persist updated preferences to the database
            await dbContext.SaveChangesAsync(cancellationToken);

            // Transform: Build response from updated preferences
            return new Response
            {
                EnableSms = profile.Notifications.EnableSms,
                EnableEmail = profile.Notifications.EnableEmail,
                EnableNewsfeeds = profile.Notifications.EnableNewsfeeds
            };
        }
    }
}
