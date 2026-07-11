using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;

using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Identity.Domain.Roles;

namespace Shared.Security.Authorization.Requirements;

/// <summary>Evaluates permission requirements by resolving effective user permissions via cache/store pipeline — admin role bypasses all checks.</summary>
// Invariant: Admin role always succeeds; unauthenticated requests are skipped; failed resolution does not fail open (denies instead).
// Context: Authorization decisions must never throw — empty permission set results in denial (Threat TMT-AUTH-001).
// Boundary: Handler → PermissionService — ASP.NET AuthorizationHandler boundary; never accesses store or cache directly.
public partial class PermissionRequirementAuthorizationHandler(
    IPermissionService permissionService,
    ILogger<PermissionRequirementAuthorizationHandler> logger)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Guard: skip unauthenticated requests — no identity to evaluate
        if (context.User.Identity?.IsAuthenticated != true)
            return;

        // Call: extract User ID from the authenticated principal (NameIdentifier or JWT sub claim)
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrEmpty(userId))
        {
            Loggers.LogNoNameIdentifier(logger);
            return;
        }

        // Validate: identifier must be a valid Guid to proceed
        if (!Guid.TryParse(userId, out Guid parsedUserId))
        {
            Loggers.LogInvalidNameIdentifier(logger, userId);
            return;
        }

        // Guard: admin role bypasses all permission checks — policy override
        if (context.User.IsInRole(RoleConstant.Defaults.Admin))
        {
            Loggers.LogAdminBypass(logger, parsedUserId, requirement.Permission);
            context.Succeed(requirement);
            return;
        }

        // Call: resolve effective permissions via service (module boundary: Handler → PermissionService)
        Result<HashSet<string>> permissionsResult = await permissionService.GetEffectiveUserPermissionsAsync(parsedUserId);

        if (permissionsResult.IsFailure)
        {
            // Log: record resolution failure — deny rather than fail open
            Loggers.LogCacheAccessFailed(logger, parsedUserId, permissionsResult.Message);
            return;
        }

        HashSet<string> permissions = permissionsResult.Value;

        // Validate: check if resolved permissions contain the required one
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
