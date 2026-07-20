using Microsoft.AspNetCore.Identity;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Store.Passwords.Change;

public static partial class ChangePassword
{
    public record Command(Request Request) : ICommand;

    /// <summary>
    /// Handles the <see cref="Command"/> to change the current user's password.
    /// </summary>
    public class CommandHandler(
        ICurrentUser currentUser,
        ISystemDateTime dateTime,
        UserManager<User> userManager,
        INotificationService notificationService,
        ILogger<CommandHandler> logger) : ICommandHandler<Command>
    {
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Changes the current user's password. Validates the current password, applies the new password via Identity,
        /// records the audit timestamp, and sends a password-changed notification.
        /// </summary>
        /// <param name="command">The command containing current and new passwords.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating success, or not-found/password-mismatch error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to persist the password change.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Load: Retrieve the current user to verify they exist
            var user = await userManager.FindByIdAsync(currentUser.UserId!);
            if (user is null)
                return UserResult.Failure.NotFound;

            // Validate: Confirm the current password matches before allowing a change
            var isCurrentPasswordValid = await userManager.CheckPasswordAsync(user, request.CurrentPassword);
            if (!isCurrentPasswordValid)
                return UserResult.Failure.PasswordMismatch;

            // Call: Apply the new password via Identity
            var identityResult =
                await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!identityResult.Succeeded)
                return identityResult.ToResult();

            AuditableBehavior.Touch(user, dateTime.UtcNow);

            // Call: Persist the updated audit timestamp
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            // Call: Notify the user of the password change for security awareness
            await SendPasswordChangedNotificationAsync(user);

            return Result.Accepted(UserResult.Success.PasswordChanged);
        }

        private async Task SendPasswordChangedNotificationAsync(User user)
        {
            var message = NotificationMessageBuilder
                .ForUseCase(NotificationUseCase.PasswordChanged)
                .To(NotificationRecipient.Create(user.Email!, user.FirstName), NotificationChannel.Email)
                .AddParam(NotificationParameterType.UserFirstName, user.FirstName);

            var result = await notificationService.SendAsync(message.Value, default);

            if (result.IsSuccess)
            {
                UserLoggers.Passwords.PasswordChanged(logger, user.Id, user.Email!, dateTime.UtcNow.DateTime);
            }
        }
    }
}