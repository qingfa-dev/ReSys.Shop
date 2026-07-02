using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Admin.Roles.Shared.Mappings;

using Shared.Security.Identity.Domain.Roles;

namespace Module.Identity.Features.Admin.Roles.Get.ById;

/// <summary>
/// Defines the use case for retrieving a role by its ID.
/// </summary>
public static partial class GetRoleById
{
    public record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(RoleManager<Role> roleManager)
        : IQueryHandler<Query, Response>
    {
        // Contract: pre=request!=null, post=result!=null
        /// <summary>
        /// Handles the query to retrieve a role by its ID.
        /// </summary>
        /// <param name="request">The query containing the role ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the role's details or an error if the role is not found.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Check: Attempt to find the role by its ID.
            var role = await roleManager.FindByIdAsync(request.Id.ToString());

            if (role is null)
                return RoleResult.Failure.NotFound;

            // Map: Convert the role entity to the response DTO.
            return role.MapToDetail<Response>();
        }
    }
}
