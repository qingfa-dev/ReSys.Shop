using Microsoft.AspNetCore.Identity;

using Shared.Application.Domain.Concerns.Auditable;

namespace Shared.Security.Identity.Domain.Users.Keys;

/// <summary>
/// Represents passkey data for a user in the system.
/// Implements <see cref="IAuditable"/>.
/// </summary>
public class UserPasskey : IdentityUserPasskey<Guid>
{
    /// <summary>
    /// Required for EF Core and Identity instantiation.
    /// </summary>
    public UserPasskey()
    {
    }


    /// <summary>
    /// Gets or sets the associated user.
    /// </summary>
    public User User { get; set; } = null!;
}
