using Microsoft.AspNetCore.Identity;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Store.Passwords.Reset;

public static partial class ResetPassword
{
    public record Command(Request Request) : ICommand;

    public class CommandHandler(
        UserManager<User> userManager,
        INotificationService notificationService,
        ILogger<CommandHandler> logger) : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var user = await userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
                return UserResult.Failure.InvalidToken;

            var identityResult = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!identityResult.Succeeded)
                return identityResult.ToResult();

            user.ModifiedAtUtc = DateTimeOffset.UtcNow;

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

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
                UserLoggers.Passwords.PasswordReset(logger, user.Id, user.Email!, DateTime.UtcNow);
            }
        }
    }
}
