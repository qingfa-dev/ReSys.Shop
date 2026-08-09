using Module.Identity.Features.Shared.Admin.Roles.Shared.Models;

using Shared.Security.Identity.Domain.Roles;

namespace Module.Identity.Features.Shared.Admin.Roles.Shared.Mappings;

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