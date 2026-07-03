using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Options;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Store.Emails.Resend;

public static partial class ResendEmailVerification
{
    public sealed record Command(Request Request) : ICommand;

    public sealed class CommandHandler(
        ISystemDateTime systemDateTime,
        UserManager<User> userManager,
        INotificationService notificationService,
        IOptions<NotificationSetting> NotificationSetting)
        : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var user = await userManager.FindByEmailAsync(request.Email);

            if (user is null)
                return Result.NoContent();

            if (user.EmailConfirmed)
                return Result.NoContent();

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var verificationUrl = BuildVerificationPath(user.Id, token);

            user.ModifiedAtUtc = systemDateTime.UtcNow;
            user.ModifiedBy = "System";

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            await SendVerificationNotificationAsync(user, verificationUrl);

            return Result.NoContent();
        }

        private async Task SendVerificationNotificationAsync(User user, string verificationUrl)
        {
            var baseUrl = NotificationSetting.Value.ApplicationUrl;
            var fullUrl = $"{baseUrl}/{verificationUrl}";

            var message = NotificationMessageBuilder
                .ForUseCase(NotificationUseCase.EmailVerificationRequested)
                .To(NotificationRecipient.Create(user.Email!, user.FirstName), NotificationChannel.Email)
                .AddParam(NotificationParameterType.UserFirstName, user.FirstName)
                .AddParam(NotificationParameterType.VerificationUrl, fullUrl);

            await notificationService.SendAsync(message.Value, default);
        }
    }

    internal static string BuildVerificationPath(Guid userId, string token)
    {
        var encodedToken = token.ToBase64();
        const string path = "verify-email";

        return $"{path}?userId={userId}&token={encodedToken}";
    }
}
