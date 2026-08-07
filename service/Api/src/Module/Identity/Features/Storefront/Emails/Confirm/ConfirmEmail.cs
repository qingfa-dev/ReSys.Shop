using Microsoft.AspNetCore.Identity;

using Module.Profile.Features.Storefront.Profiles.Create;
using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Storefront.Emails.Confirm;

public static partial class ConfirmEmail
{
    public sealed record Command(Request Request) : ICommand;

    /// <summary>
    /// Handles the <see cref="Command"/> to confirm an email verification or change.
    /// </summary>
    public sealed class CommandHandler(
        UserManager<User> userManager,
        ICurrentUser currentUser,
        INotificationService notificationService,
        ILogger<CommandHandler> logger,
        IMediator mediator)
        : ICommandHandler<Command>
    {
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Confirms a pending email verification or email change. Decodes the token and optional new email,
        /// applies the confirmation via Identity, updates audit timestamps, sends a welcome notification
        /// for first-time verification, and creates an initial user profile.
        /// </summary>
        /// <param name="command">The command containing the confirmation token and optional new email.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating success, or an invalid-token/not-found error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to persist the confirmation.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            string? decodedNewEmail = null;

            // Validate: Decode the confirmation token, reject if malformed
            if (!Base64Converter.TryFromBase64Url(request.Token, out string decodedToken))
            {
                return UserResult.Failure.InvalidToken;
            }

            // Validate: Decode the optional new email for email-change scenarios
            if (!string.IsNullOrEmpty(request.NewEmail))
            {
                if (!Base64Converter.TryFromBase64Url(request.NewEmail, out var tempEmail))
                {
                    return UserResult.Failure.InvalidToken;
                }
                decodedNewEmail = tempEmail;
            }

            // Load: Retrieve the user to confirm their identity exists
            var user = await userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            var isEmailChange = !string.IsNullOrWhiteSpace(decodedNewEmail);

            // Check: Skip confirmation if the email is already verified (idempotent)
            if (user.EmailConfirmed && !isEmailChange)
                return Result.NoContent();

            // Call: Apply the email operation — change or confirm — via Identity
            var identityResult = isEmailChange
                ? await userManager.ChangeEmailAsync(user, decodedNewEmail!, decodedToken)
                : await userManager.ConfirmEmailAsync(user, decodedToken);

            if (!identityResult.Succeeded)
                return identityResult.ToResult();

            user.ModifiedAtUtc = DateTimeOffset.UtcNow;

            // Call: Persist the updated audit timestamp
            var updateResult = await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            // Log: Record the confirmation type (email change vs. initial verification) for audit trail
            if (isEmailChange)
            {
                UserLoggers.Emails.EmailChangeConfirmed(logger, UserId: user.Id, Email: user.Email!, Timestamp: DateTime.UtcNow, ActionBy: currentUser.UserName);
            }
            else
            {
                UserLoggers.Emails.EmailVerified(logger, UserId: user.Id, Email: user.Email!, Timestamp: DateTime.UtcNow, ActionBy: currentUser.UserName);
            }

            // Best-effort: profile creation always fires; welcome only for first-time verification.
            // Failures in CreateUserProfileAsync are logged internally and do not propagate.
            await Task.WhenAll(
                CreateUserProfileAsync(user, cancellationToken),
                isEmailChange ? Task.CompletedTask : SendWelcomeNotificationAsync(user));

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

        private async Task CreateUserProfileAsync(User user, CancellationToken cancellationToken)
        {
            try
            {
                var profileResult = await mediator.Send(
                    new CreateUserProfileCommand
                    {
                        UserId = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email!
                    },
                    cancellationToken);

                if (profileResult.IsFailure)
                {
                    var errors = string.Join("; ", profileResult.Errors.Select(e => $"{e.Code}: {e.Message}"));
                    UserLoggers.Profiles.ProfileCreationFailed(logger, user.Id, errors);
                }
                else
                {
                    UserLoggers.Profiles.ProfileCreated(logger, user.Id, profileResult.Value.ProfileId);
                }
            }
            catch (Exception ex)
            {
                UserLoggers.Profiles.ProfileCreationFailed(logger, user.Id, ex.Message);
            }
        }
    }
}