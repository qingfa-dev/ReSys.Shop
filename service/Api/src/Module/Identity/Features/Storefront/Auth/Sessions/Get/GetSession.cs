using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Shared.Storefront.Shared.Mappings;

using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Shared.Storefront.Auth.Sessions.Get;

/// <summary>
/// Defines the use case for retrieving the current user's session.
/// </summary>
public static partial class GetSession
{
    public sealed record Query : IQuery<Response>;

    public partial class QueryHandler(
        ICurrentUser currentUser,
        IPermissionService permissionService,
        UserManager<User> userManager)
        : IQueryHandler<Query, Response>
    {
        // Contract: pre=request!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Resolves the current authenticated user and returns their session payload including roles and effective permissions.
        /// </summary>
        /// <param name="request">The query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing user ID, roles, and permissions, or an auth-required error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store query fails.</exception>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Check: Verify the caller is authenticated
            if (!currentUser.IsAuthenticated)
                return UserResult.Failure.Unauthorized;

            // Load: Retrieve the authenticated user
            var user = await userManager.FindByIdAsync(currentUser.UserId!);
            if (user is null)
                return UserResult.Failure.NotFound;

            // Compute: Resolve assigned roles for the user
            var roles = await userManager.GetRolesAsync(user);

            // Call: Fetch effective permissions via permission service
            var permissions = await permissionService.GetEffectiveUserPermissionsAsync(user.Id, cancellationToken);
            var effectivePermissions = permissions.IsSuccess ? permissions.Value : new HashSet<string>();

            var response = (user, roles.ToArray(), effectivePermissions).MapToSessionResponse<Response>();

            return Result<Response>.Ok(response, UserResult.Success.SessionRetrieved);
        }
    }

}