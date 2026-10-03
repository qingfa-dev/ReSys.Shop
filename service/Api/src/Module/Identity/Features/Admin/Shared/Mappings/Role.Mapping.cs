using Module.Identity.Features.Admin.Shared.Models;
using Shared.Security.Identity.Domain.Roles;

namespace Module.Identity.Features.Admin.Shared.Mappings;

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

/// <summary>
/// Provides extension methods for mapping between <see cref="Role"/> entities and various DTOs.
/// </summary>
public static partial class RoleMapping
{
    /// <summary>
    /// Converts a <see cref="Role"/> entity to a detailed response DTO.
    /// </summary>
    /// <typeparam name="T">The type of the detailed response DTO, which must inherit from <see cref="RoleDetailResponse"/>.</typeparam>
    /// <param name="role">The <see cref="Role"/> entity to convert.</param>
    /// <returns>A new instance of the detailed response DTO populated with data from the role.</returns>
    public static T MapToDetail<T>(this Role role) where T : RoleDetailResponse, new()
    {
        return new T
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            Description = role.Description,
            IsSystem = role.IsSystem,
            CreatedAtUtc = role.CreatedAtUtc,
            ModifiedAtUtc = role.ModifiedAtUtc,
            CreatedBy = role.CreatedBy,
            ModifiedBy = role.ModifiedBy
        };
    }

    /// <summary>
    /// Converts a <see cref="Role"/> entity to a list item response DTO.
    /// </summary>
    /// <typeparam name="T">The type of the list item response DTO, which must inherit from <see cref="RoleListResponse"/>.</typeparam>
    /// <param name="role">The <see cref="Role"/> entity to convert.</param>
    /// <returns>A new instance of the list item response DTO populated with data from the role.</returns>
    public static T MapToListItem<T>(this Role role) where T : RoleListResponse, new()
    {
        return new T
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            Description = role.Description,
            IsSystem = role.IsSystem
        };
    }
}
