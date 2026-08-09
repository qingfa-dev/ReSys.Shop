using Microsoft.AspNetCore.Identity;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Shared.Storefront.Passwords.Reset;

public static partial class ResetPassword
{
    public record Command(Request Request) : ICommand;

    /// <summary>
    /// Handles the <see cref="Command"/> to reset a user's password.
    /// </summary>
    public class CommandHandler(
        UserManager<User> userManager,
        ISystemDateTime dateTime,
        INotificationService notificationService,
        ILogger<CommandHandler> logger) : ICommandHandler<Command>
    {
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Resets a user's password using a password-reset token. Validates the token, applies the new password,
        /// records the audit timestamp, and sends a confirmation notification.
        /// </summary>
        /// <param name="command">The command containing user ID, reset token, and new password.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating success or an invalid-token error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to persist the password reset.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Load: Retrieve the user by ID to verify they exist
            var user = await userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
                return UserResult.Failure.InvalidToken;

            // Call: Apply the password reset via Identity with the provided token
            var identityResult = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!identityResult.Succeeded)
                return identityResult.ToResult();

            user.ModifiedAtUtc = dateTime.UtcNow;

            // Call: Persist the updated audit timestamp
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            // Call: Send confirmation notification to the user for security awareness
            await SendPasswordResetConfirmedNotificationAsync(user);

            return Result.Accepted(UserResult.Success.PasswordReset);
        }

        private async Task SendPasswordResetConfirmedNotificationAsync(User user)
        {
            var message = NotificationMessageBuilder
                .ForUseCase(NotificationUseCase.PasswordChanged)
                .To(NotificationRecipient.Create(user.Email!, user.FirstName), NotificationChannel.Email)
                .AddParam(NotificationParameterType.UserFirstName, user.FirstName);

            var result = await notificationService.SendAsync(message.Value, default);

            if (result.IsSuccess)
            {
                UserLoggers.Passwords.PasswordReset(logger, user.Id, user.Email!, dateTime.UtcNow.DateTime);
            }
        }
    }
}