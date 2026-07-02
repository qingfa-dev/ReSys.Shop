using Microsoft.AspNetCore.Identity;

using Shared.Governance.Conventions;
using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Store.Emails.Confirm;

public static partial class ConfirmEmail
{
    public sealed record Command(Request Request) : ICommand;

    public sealed class CommandHandler(
        UserManager<User> userManager,
        ICurrentUser currentUser,
        INotificationService notificationService,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            string? decodedNewEmail = null;

            if (!Base64Converter.TryFromBase64Url(request.Token, out string decodedToken))
            {
                return UserResult.Failure.InvalidToken;
            }

            if (!string.IsNullOrEmpty(request.NewEmail))
            {
                if (!Base64Converter.TryFromBase64Url(request.NewEmail, out var tempEmail))
                {
                    return UserResult.Failure.InvalidToken;
                }
                decodedNewEmail = tempEmail;
            }

            var user = await userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            if (user.EmailConfirmed)
                return Result.NoContent();

            var isEmailChange = !string.IsNullOrWhiteSpace(decodedNewEmail);

            var identityResult = isEmailChange
                ? await userManager.ChangeEmailAsync(user, decodedNewEmail!, decodedToken)
                : await userManager.ConfirmEmailAsync(user, decodedToken);

            if (!identityResult.Succeeded)
                return identityResult.ToResult();

            user.ModifiedAtUtc = DateTimeOffset.UtcNow;

            var updateResult = await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            if (isEmailChange)
            {
                UserLoggers.Emails.EmailChangeConfirmed(logger, UserId: user.Id, Email: user.Email!, Timestamp: DateTime.UtcNow, ActionBy: currentUser.UserName);
            }
            else
            {
                UserLoggers.Emails.EmailVerified(logger, UserId: user.Id, Email: user.Email!, Timestamp: DateTime.UtcNow, ActionBy: currentUser.UserName);
                await SendWelcomeNotificationAsync(user);
            }

            return Result.NoContent();
        }

        private async Task SendWelcomeNotificationAsync(User user)
        {
            var message = NotificationMessageBuilder
                .ForUseCase(NotificationUseCase.WelcomeSent)
                .To(NotificationRecipient.Create(user.Email!, user.FirstName), NotificationChannel.Email)
                .AddParam(NotificationParameterType.UserFirstName, user.FirstName);

            await notificationService.SendAsync(message.Value, default);
        }
    }
}
