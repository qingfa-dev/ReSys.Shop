using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Options;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Store.Auth.Register;

/// <summary>
/// Defines the use case for email-based user registration.
/// </summary>
public static partial class EmailRegister
{
    public record Command(Request Request) : ICommand<Response>;

    public class CommandHandler(
        UserManager<User> userManager,
        INotificationService notificationService,
        IOptions<NotificationSetting> notificationOptions,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Registers a new user account with email and password. Validates uniqueness of email and username,
        /// creates the Identity user, assigns the default role, generates an email verification token,
        /// and sends a verification notification.
        /// </summary>
        /// <param name="command">The command containing registration details.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the registered user's ID, email, and success message, or a validation error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to persist the new user.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser is not null)
                return UserResult.Failure.EmailDuplicate;

            if (string.IsNullOrWhiteSpace(request.FirstName))
                return UserResult.Failure.FirstNameRequired;

            var trimmedUsername = request.UserName.Trim();
            var existingByUsername = await userManager.FindByNameAsync(trimmedUsername);
            if (existingByUsername is not null)
                return UserResult.Failure.UsernameDuplicate;

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email.Trim().ToLowerInvariant(),
                UserName = trimmedUsername.ToLowerInvariant(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName?.Trim(),
                PhoneNumber = request.Phone?.Trim(),
                IsActive = UserConstant.Defaults.IsActive,
                EmailConfirmed = UserConstant.Defaults.EmailConfirmed,
                CreatedAtUtc = DateTimeOffset.UtcNow
            };

            var identityResult = await userManager.CreateAsync(user, request.Password);
            if (!identityResult.Succeeded)
                return identityResult.ToResult<Response>();

            var roleResult = await userManager.AddToRoleAsync(user, RoleConstant.Defaults.User);
            if (!roleResult.Succeeded)
                return roleResult.ToResult<Response>();

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var verificationUrl = BuildVerificationPath(user.Id, token);

            AuditableBehavior.Touch(user);

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult<Response>();

            await SendEmailVerificationNotificationAsync(user, request.Email, verificationUrl);

            return new Response(
                user.Id,
                user.Email,
                UserResult.Success.Registered);
        }

        internal static string BuildVerificationPath(Guid userId, string token)
        {
            var encodedToken = token.ToBase64Url();
            const string path = "verify-email";
            return $"{path}?userId={userId}&token={encodedToken}";
        }

        private async Task SendEmailVerificationNotificationAsync(User user, string email, string verificationUrl)
        {
            var fullUrl = $"{notificationOptions.Value.ApplicationUrl}/{verificationUrl}";

            var message = NotificationMessageBuilder
                .ForUseCase(NotificationUseCase.EmailVerificationRequested)
                .To(NotificationRecipient.Create(email, user.FirstName), NotificationChannel.Email)
                .AddParam(NotificationParameterType.UserFirstName, user.FirstName)
                .AddParam(NotificationParameterType.VerificationUrl, fullUrl);

            var result = await notificationService.SendAsync(message.Value, default);

            if (result.IsSuccess)
                UserLoggers.Passwords.ConfirmationSent(logger, email);
            else
                UserLoggers.Passwords.ConfirmationSentFailed(logger, email, string.Join(", ", result.Errors.Select(f => f.Message)));
        }
    }
}