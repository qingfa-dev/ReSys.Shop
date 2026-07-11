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
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Resends the email verification token for a user who has not yet confirmed their email.
        /// Silently returns NoContent if the user is unknown or already confirmed to avoid leaking user existence.
        /// </summary>
        /// <param name="command">The command containing the user's email address.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A NoContent result indicating the notification was sent or suppressed.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to persist the audit timestamp.</exception>
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
