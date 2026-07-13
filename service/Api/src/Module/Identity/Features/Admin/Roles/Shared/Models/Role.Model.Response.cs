namespace Module.Identity.Features.Admin.Roles.Shared.Models;

/// <summary>
/// Represents a detailed response for a role, including audit information.
/// Inherits common role properties from <see cref="RoleParameter"/>.
/// </summary>
public class RoleDetailResponse : RoleParameter
{
    /// <summary>
    /// Gets or initializes the unique identifier of the role.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or initializes a value indicating whether the role is a system role.
    /// System roles usually have restricted modification or deletion.
    /// </summary>
    public bool IsSystem { get; init; }

    /// <summary>
    /// Gets or initializes the UTC date and time when the role was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>
    /// Gets or initializes the UTC date and time when the role was last modified, if applicable.
    /// </summary>
    public DateTimeOffset? ModifiedAtUtc { get; init; }

    /// <summary>
    /// Gets or initializes the identifier of the user who created the role.
    /// </summary>
    public string? CreatedBy { get; init; }

    /// <summary>
    /// Gets or initializes the identifier of the user who last modified the role, if applicable.
    /// </summary>
    public string? ModifiedBy { get; init; }
}

/// <summary>
/// Represents a simplified response for a role, typically used in lists.
/// </summary>
public class RoleListResponse : RoleParameter
{
    /// <summary>
    /// Gets or initializes the unique identifier of the role.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or initializes a value indicating whether the role is a system role.
    /// </summary>
    public bool IsSystem { get; init; }
}