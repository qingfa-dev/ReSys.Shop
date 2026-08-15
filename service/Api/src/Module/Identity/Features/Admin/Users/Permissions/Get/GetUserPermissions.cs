using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Shared.Admin.Permissions.Shared.Mappings;

using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Shared.Admin.Users.Permissions.Get;

public static partial class GetUserPermissions
{
    /// <summary>
    /// Represents the query to retrieve all available permissions and their assignment status (direct and inherited) for a specific user.
    /// </summary>
    /// <param name="Id">The unique identifier of the user.</param>
    public sealed record Query(Guid Id) : IQuery<Response>;

    /// <summary>
    /// Handles the <see cref="Query"/> to retrieve user permissions.
    /// </summary>
    public sealed class QueryHandler(
        UserManager<User> userManager,
        IPermissionService permissionService)
        : IQueryHandler<Query, Response>
    {
        // Contract: pre=request!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Retrieves the full permission tree for a user, marking each permission as direct or inherited through roles.
        /// </summary>
        /// <param name="request">The query containing the user ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the permission tree with assignment status, or NotFound if the user does not exist.</returns>
        /// <exception cref="DbUpdateException">Thrown when the underlying identity store fails.</exception>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.Id.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            var permissionsResult = await permissionService.GetEffectiveUserPermissionsAsync(user.Id, cancellationToken);
            if (permissionsResult.IsFailure)
                return permissionsResult.Errors;

            var effectivePermissions = permissionsResult.Value;

            return PermissionContext.All.MapToPermissionComposite<Response, CategoryResponse, ResourceResponse, PermissionItemResponse>(effectivePermissions);
        }
    }
}