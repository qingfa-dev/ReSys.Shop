using Microsoft.AspNetCore.Identity;

using Module.Profile.Domain;

using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Store.Auth.Sessions.Get;

/// <summary>
/// Defines the use case for retrieving the current user's session.
/// </summary>
public static partial class GetSession
{
    // ============  QUERY  ============
    public sealed record Query : IQuery<Response>;

    // ============  QUERY HANDLER ============
    public partial class QueryHandler(
        ICurrentUser currentUser,
        IPermissionService permissionService,
        UserManager<User> userManager)
        : IQueryHandler<Query, Response>
    {
        // Contract: pre=request!=null, post=result!=null
        /// <summary>
        /// Handles the query to retrieve the current user's session.
        /// </summary>
        /// <param name="request">The query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the current user's session data or an error.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Check: Ensure user is authenticated
            if (!currentUser.IsAuthenticated)
                return UserProfileResult.Failure.AuthRequired;

            // Query: Resolve current user from identity store
            var user = await userManager.FindByIdAsync(currentUser.UserId!);
            if (user is null)
                return UserResult.Failure.NotFound;

            // Query: Retrieve assigned roles for the user
            var roles = await userManager.GetRolesAsync(user);

            // Query: Retrieve assigned permissions for the users
            var permissions = await permissionService.GetEffectiveUserPermissionsAsync(user.Id, cancellationToken);

            // Map: Build the profile response with user data, roles, and permissions
            var response = new Response
            {
                Id = user.Id,
                Roles = roles.ToArray(),
                Permissions = permissions.IsSuccess ? [.. permissions.Value] : []
            };

            return response;
        }
    }

}