using Module.Identity.Features.Admin.Roles.Shared.Models;

using Shared.Security.Identity.Domain.Roles;

namespace Module.Identity.Features.Admin.Roles.Shared.Mappings;

/// <summary>
/// Provides extension methods for mapping between <see cref="Role"/> entities and various DTOs.
/// </summary>
public static partial class RoleMapping
{
    /// <summary>
    /// Converts a role creation/update request DTO to a <see cref="Role"/> entity.
    /// </summary>
    /// <typeparam name="T">The type of the request DTO, which must inherit from <see cref="RoleRequest"/>.</typeparam>
    /// <param name="request">The request DTO to convert.</param>
    /// <returns>A new <see cref="Role"/> entity populated with data from the request.</returns>
    public static Role MapToDomain<T>(this T request) where T : RoleRequest
    {
        return new Role
        {
            Name = request.Name,
            Description = request.Description,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Updates an existing <see cref="Role"/> entity with data from a request DTO.
    /// </summary>
    /// <typeparam name="T">The type of the request DTO, which must inherit from <see cref="RoleRequest"/>.</typeparam>
    /// <param name="role">The <see cref="Role"/> entity to update.</param>
    /// <param name="request">The request DTO containing the updated data.</param>
    public static void MapToDomain<T>(this T request, Role role) where T : RoleRequest
    {
        role.Name = request.Name;
        role.Description = request.Description;
        role.ModifiedAtUtc = DateTimeOffset.UtcNow;
    }
}