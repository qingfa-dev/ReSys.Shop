using Microsoft.AspNetCore.Identity;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Permissions;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Users.Permissions.Assign;

public static partial class AssignUserPermissions
{
    /// <summary>
    /// Represents the command to assign permissions to a user.
    /// </summary>
    /// <param name="Id">The unique identifier of the user.</param>
    /// <param name="Request">The request containing the list of permission identifiers to assign.</param>
    public sealed record Command(Guid Id, Request Request) : ICommand;

    /// <summary>
    /// Handles the <see cref="Command"/> to assign permissions to a user.
    /// </summary>
    public sealed class CommandHandler(
        ISystemDateTime dateTime,
        UserManager<User> userManager,
        IPermissionService permissionService,
        ICurrentUser currentUser,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Assigns direct permissions to a user. Validates caller authority for each requested permission,
        /// computes delta against existing claims, persists additions, and invalidates the user's permission cache.
        /// </summary>
        /// <param name="command">The command with user ID and permissions to assign.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating success or unauthorized/not-found/assign-denied error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to persist user claims.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.UserId, out Guid currentUserId))
                return UserResult.Failure.Unauthorized;

            var user = await userManager.FindByIdAsync(command.Id.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            var requestedPermissions = command.Request.Permissions
                .Where(p => PermissionContext.All.Select(p => p.Identifier).Contains(p))
                .ToList();

            if (requestedPermissions.Count == 0)
                return Result.Ok();

            var authResult =
                await permissionService.HasAllPermissionsAsync(currentUserId, requestedPermissions, cancellationToken);

            if (authResult.IsFailure || !authResult.Value)
            {
                return UserResult.Failure.AssignDenied(requestedPermissions.First());
            }

            var existingClaims = await userManager.GetClaimsAsync(user);
            var existingPermissionValues = existingClaims
                .Where(c => c.Type == PermissionMetadataConstant.ClaimType)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var permissionsToAdd = requestedPermissions
                .Where(p => !existingPermissionValues.Contains(p))
                .ToList();

            if (permissionsToAdd.Count == 0)
                return Result.Ok();

            var addResult =
                await permissionService.AddUserDirectPermissionsAsync(user.Id, permissionsToAdd, cancellationToken);
            if (addResult.IsFailure)
                return addResult;

            AuditableBehavior.Touch(user, dateTime.UtcNow);

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            if (logger.IsEnabled(LogLevel.Debug))
            {
                var permissions = string.Join(", ", permissionsToAdd);
                UserLoggers.Permissions.PermissionsAssigned(logger, UserName: user.UserName!, UserId: user.Id,
                    Permissions: permissions, ActionBy: currentUser.UserName);
            }

            await OnPermissionsChangedAsync(user, cancellationToken);

            return Result.Ok();
        }

        private async Task OnPermissionsChangedAsync(User user, CancellationToken ct)
        {
            await permissionService.InvalidateUserPermissionsAsync(user.Id, ct);
        }
    }
}