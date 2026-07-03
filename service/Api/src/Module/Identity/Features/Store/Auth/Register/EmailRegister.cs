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
        // Contract: pre=command!=null, post=result!=null
        /// <summary>
        /// Handles the command for email-based user registration.
        /// </summary>
        /// <param name="command">The command containing registration details.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the registered user's details or an error.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Check: Ensure the email is not already registered
            var existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser is not null)
                return UserResult.Failure.EmailDuplicate;

            // Check: Verify required name fields are present
            if (string.IsNullOrWhiteSpace(request.FirstName))
                return UserResult.Failure.FirstNameRequired;

            // Check: Ensure the username is not already taken
            var trimmedUsername = request.UserName.Trim();
            var existingByUsername = await userManager.FindByNameAsync(trimmedUsername.ToLowerInvariant());
            if (existingByUsername is not null)
                return UserResult.Failure.UsernameDuplicate;

            // Create: Instantiate a new User entity from request data
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

            // Persist: Create user account with hashed password via Identity
            var identityResult = await userManager.CreateAsync(user, request.Password);
            if (!identityResult.Succeeded)
                return identityResult.ToResult<Response>();

            // Update: Assign default application role to the new user
            var roleResult = await userManager.AddToRoleAsync(user, RoleConstant.Defaults.User);
            if (!roleResult.Succeeded)
                return roleResult.ToResult<Response>();

            // Create: Generate email verification token and secure verification URL
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var verificationUrl = BuildVerificationPath(user.Id, token);

            // Update: Record verification request
            AuditableBehavior.Touch(user);

            // Persist: Save user state
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult<Response>();

            // Notify: Send email verification
            await SendEmailVerificationNotificationAsync(user, request.Email, verificationUrl);

            // Map: Return registration success response
            return new Response(
                user.Id,
                user.Email,
                UserResult.Success.Registered);
        }

        internal static string BuildVerificationPath(Guid userId, string token)
        {
            var encodedToken = token.ToBase64();
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