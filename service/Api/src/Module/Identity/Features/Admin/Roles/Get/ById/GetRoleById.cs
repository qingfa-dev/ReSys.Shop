using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Admin.Shared.Mappings;

using Shared.Security.Identity.Domain.Roles;

namespace Module.Identity.Features.Shared.Admin.Roles.Get.ById;

/// <summary>
/// Defines the use case for retrieving a role by its ID.
/// </summary>
public static partial class GetRoleById
{
    public record Query(Guid Id) : IQuery<Response>;

    /// <summary>
    /// Handles the <see cref="Query"/> to retrieve a role by its ID.
    /// </summary>
    public sealed class QueryHandler(RoleManager<Role> roleManager)
        : IQueryHandler<Query, Response>
    {
        /// <summary>
        /// Retrieves a role's details by its ID or returns NotFound if the role does not exist.
        /// </summary>
        /// <param name="request">The query containing the role ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the role's details or NotFound error.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Load: Look up role by ID to verify it exists in the identity store
            var role = await roleManager.FindByIdAsync(request.Id.ToString());

            // Check: Return NotFound if no role matches the requested ID
            if (role is null)
                return RoleResult.Failure.NotFound;

            // Transform: Map domain entity to response for API consumption
            return role.MapToDetail<Response>();
        }
    }
}