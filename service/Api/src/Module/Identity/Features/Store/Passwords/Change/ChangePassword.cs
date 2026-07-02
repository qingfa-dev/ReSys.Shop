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

    public class CommandHandler(
        ICurrentUser currentUser,
        ISystemDateTime dateTime,
        UserManager<User> userManager,
        INotificationService notificationService,
        ILogger<CommandHandler> logger) : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var user = await userManager.FindByIdAsync(currentUser.UserId!);
            if (user is null)
                return UserResult.Failure.NotFound;

            var isCurrentPasswordValid = await userManager.CheckPasswordAsync(user, request.CurrentPassword);
            if (!isCurrentPasswordValid)
                return UserResult.Failure.PasswordMismatch;

            var identityResult =
                await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!identityResult.Succeeded)
                return identityResult.ToResult();

            AuditableBehavior.Touch(user, dateTime.UtcNow);

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

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
                UserLoggers.Passwords.PasswordChanged(logger, user.Id, user.Email!, DateTime.UtcNow);
            }
        }
    }
}
