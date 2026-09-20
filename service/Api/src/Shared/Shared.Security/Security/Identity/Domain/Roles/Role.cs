using Microsoft.AspNetCore.Identity;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Security.Identity.Domain.Roles.Claims;
using Shared.Security.Identity.Domain.Users.Roles;

namespace Shared.Security.Identity.Domain.Roles;

public class Role : IdentityRole<Guid>, IAuditable
{
    #region Properties
    /// <summary>
    /// Gets or sets the description of the role.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a system-protected role.
    /// System roles cannot be modified or deleted.
    /// </summary>
    public bool IsSystem { get; set; }

    #endregion Properties


    #region Auditing
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Navigation

    // Navigation

    /// <summary>
    /// Gets or sets the collection of user-role mappings for this role.
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of role claims (permissions) for this role.
    /// </summary>
    public ICollection<RoleClaim> RoleClaims { get; set; } = [];

    #endregion Navigation
}