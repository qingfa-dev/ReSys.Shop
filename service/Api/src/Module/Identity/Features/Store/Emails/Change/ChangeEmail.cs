using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Options;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Store.Emails.Change;

public static partial class ChangeEmail
{
    public sealed record Command(Request Request) : ICommand;

    public sealed class CommandHandler(
        ICurrentUser currentUser,
        UserManager<User> userManager,
        INotificationService notificationService,
        IOptions<NotificationSetting> notificationSetting)
        : ICommandHandler<Command>
    {
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Requests an email address change for the current user. Validates the current password,
        /// checks the new email is not already taken by another user, generates a change token,
        /// and sends a confirmation notification.
        /// </summary>
        /// <param name="command">The command containing the new email and current password.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating success or validation failure.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to persist the audit timestamp.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var user = await userManager.FindByIdAsync(currentUser.UserId!);
            if (user is null)
                return UserResult.Failure.NotFound;

            var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
                return UserResult.Failure.InvalidCredentials;

            var existingUser = await userManager.FindByEmailAsync(request.NewEmail);
            if (existingUser is not null && existingUser.Id != user.Id)
                return UserResult.Failure.EmailDuplicate;

            var token = await userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);
            var confirmationUrl = BuildConfirmPath(user.Id, token, request.NewEmail);

            AuditableBehavior.Touch(user);

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            await SendEmailChangeNotificationAsync(user, request.NewEmail, confirmationUrl);

            return Result.NoContent();
        }

        private async Task SendEmailChangeNotificationAsync(User user, string newEmail, string confirmationUrl)
        {
            var baseUrl = notificationSetting.Value.ApplicationUrl;
            var fullUrl = $"{baseUrl}/{confirmationUrl}";

            var message = NotificationMessageBuilder
                .ForUseCase(NotificationUseCase.EmailChanged)
                .To(NotificationRecipient.Create(newEmail, user.FirstName), NotificationChannel.Email)
                .AddParam(NotificationParameterType.UserFirstName, user.FirstName)
                .AddParam(NotificationParameterType.ConfirmationUrl, fullUrl);

            await notificationService.SendAsync(message.Value, default);
        }
    }

    internal static string BuildConfirmPath(Guid userId, string token, string newEmail)
    {
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(newEmail);
        const string path = "confirm-email-change";

        return $"{path}?userId={userId}&token={encodedToken}&newEmail={encodedEmail}";
    }
}