using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Options;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Shared.Storefront.Passwords.Forgot;

public static partial class RequestPasswordReset
{
    public record Command(Request Request) : ICommand;

    /// <summary>
    /// Handles the <see cref="Command"/> to request a password reset.
    /// </summary>
    public class CommandHandler(
        UserManager<User> userManager,
        ISystemDateTime dateTime,
        INotificationService notificationService,
        IOptions<NotificationSetting> NotificationSetting,
        ILogger<CommandHandler> logger) : ICommandHandler<Command>
    {
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Initiates a password reset flow for the given email. Generates a password reset token,
        /// updates the audit timestamp, and sends a reset-link notification. Silently returns NoContent
        /// for unknown or inactive users to avoid leaking account existence.
        /// </summary>
        /// <param name="command">The command containing the user's email address.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A NoContent result indicating the notification was sent or suppressed.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to persist the audit timestamp.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Validate: Reject empty email early to avoid unnecessary lookups
            if (string.IsNullOrWhiteSpace(request.Email))
                return Result.NoContent();

            // Load: Look up user by email without revealing whether the account exists
            var user = await userManager.FindByEmailAsync(request.Email);

            // Guard: Silently return NoContent for unknown users to prevent email enumeration
            if (user is null)
                return Result.NoContent();

            // Guard: Silently return NoContent for inactive users to prevent account enumeration
            if (!user.IsActive)
                return Result.NoContent();

            // Call: Generate a password reset token for the valid user
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            // Transform: Build reset URL with encoded token for the notification
            var resetPath = BuildConfirmPath(user.Id, token, user.Email!);

            user.ModifiedAtUtc = dateTime.UtcNow;

            // Log: Record the password reset request for audit trail
            UserLoggers.Passwords.PasswordResetRequested(logger, UserId: user.Id, Email: user.Email!, Timestamp: dateTime.UtcNow.DateTime);

            // Call: Persist the updated audit timestamp
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            // Catch: Handle notification delivery failure gracefully without breaking the flow
            try
            {
                // Call: Send the password reset link to the user's email
                await SendPasswordResetNotificationAsync(user, resetPath);
            }
            catch (Exception ex)
            {
                // Log: Warning for failed notification delivery — does not block the reset flow
                UserLoggers.Passwords.PasswordResetNotificationFailed(logger, user.Id, ex);
            }

            return Result.NoContent();
        }

        private async Task SendPasswordResetNotificationAsync(User user, string resetPath)
        {
            var baseUrl = NotificationSetting.Value.ApplicationUrl;
            var fullUrl = $"{baseUrl}/{resetPath}";

            var message = NotificationMessageBuilder
                .ForUseCase(NotificationUseCase.PasswordResetRequested)
                .To(NotificationRecipient.Create(user.Email!, user.FirstName), NotificationChannel.Email)
                .AddParam(NotificationParameterType.UserFirstName, user.FirstName)
                .AddParam(NotificationParameterType.ResetPasswordUrl, fullUrl);

            await notificationService.SendAsync(message.Value, default);
        }
    }

    internal static string BuildConfirmPath(Guid userId, string token, string newEmail)
    {
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(newEmail);
        const string path = "reset-password";

        return $"{path}?userId={userId}&token={encodedToken}&newEmail={encodedEmail}";
    }
}