using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Options;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Store.Passwords.Forgot;

public static partial class RequestPasswordReset
{
    public record Response(string Message);

    public record Command(Request Request) : ICommand;

    public class CommandHandler(
        UserManager<User> userManager,
        INotificationService notificationService,
        IOptions<NotificationSetting> NotificationSetting,
        ILogger<CommandHandler> logger) : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            if (string.IsNullOrWhiteSpace(request.Email))
                return Result.NoContent();

            var user = await userManager.FindByEmailAsync(request.Email);

            if (user is null)
                return Result.NoContent();

            if (!user.IsActive)
                return Result.NoContent();

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetPath = BuildConfirmPath(user.Id, token, user.Email!);

            user.ModifiedAtUtc = DateTimeOffset.UtcNow;

            UserLoggers.Passwords.PasswordResetRequested(logger, UserId: user.Id, Email: user.Email!, Timestamp: DateTime.UtcNow);

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            try
            {
                await SendPasswordResetNotificationAsync(user, resetPath);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send password reset notification to {UserId}", user.Id);
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
