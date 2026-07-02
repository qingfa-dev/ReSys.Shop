using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;

using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Identity.Domain.Roles;

namespace Shared.Security.Authorization.Requirements;

/// <summary>
/// Consolidated authorization handler that fetches user permissions directly
/// from cache/store and evaluates the permission requirement.
/// 
/// Replaces the combined flow of PermissionClaimsTransformer and PermissionAuthorizationHandler.
/// </summary>
public partial class PermissionRequirementAuthorizationHandler(
    IPermissionService permissionService,
    ILogger<PermissionRequirementAuthorizationHandler> logger)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Guard: Skip for unauthenticated requests
        if (context.User.Identity?.IsAuthenticated != true)
            return;

        // Receive: Extract User ID from the authenticated principal
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrEmpty(userId))
        {
            Loggers.LogNoNameIdentifier(logger);
            return;
        }

        // Check: Validate the extracted identifier format
        if (!Guid.TryParse(userId, out Guid parsedUserId))
        {
            Loggers.LogInvalidNameIdentifier(logger, userId);
            return;
        }

        // Bypass: Admin role automatically passes all permission checks.
        if (context.User.IsInRole(RoleConstant.Defaults.Admin))
        {
            Loggers.LogAdminBypass(logger, parsedUserId, requirement.Permission);
            context.Succeed(requirement);
            return;
        }

        // Call: Retrieve effective permissions using the optimized service
        Result<HashSet<string>> permissionsResult = await permissionService.GetEffectiveUserPermissionsAsync(parsedUserId);

        if (permissionsResult.IsFailure)
        {
            // Log: Record resolution failure
            Loggers.LogCacheAccessFailed(logger, parsedUserId, permissionsResult.Message);
            return;
        }

        HashSet<string> permissions = permissionsResult.Value;

        // Evaluate: Check if the fetched permissions contain the required one
        if (permissions.Contains(requirement.Permission))
        {
            Loggers.LogAuthorizationSucceeded(logger, parsedUserId, requirement.Permission);
            context.Succeed(requirement);
        }
        else
        {
            Loggers.LogAuthorizationFailed(logger, parsedUserId, requirement.Permission);
        }
    }
}
