using Microsoft.AspNetCore.Identity;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Security.Identity.Domain.Tokens;
using Shared.Security.Identity.Domain.Users.Claims;
using Shared.Security.Identity.Domain.Users.Keys;
using Shared.Security.Identity.Domain.Users.Logins;
using Shared.Security.Identity.Domain.Users.Roles;
using Shared.Security.Identity.Domain.Users.Tokens;

namespace Shared.Security.Identity.Domain.Users;

/// <summary>
/// Represents a user account in the system.
/// Inherits from <see cref="IdentityUser{TKey}"/> and implements <see cref="IAuditable"/>.
/// </summary>
public class User : IdentityUser<Guid>, IAuditable
{

    #region Properties

    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the user's date of birth.
    /// </summary>
    public DateTimeOffset? DateOfBirth { get; set; }

    /// <summary>
    /// Gets the user's full name (FirstName + LastName).
    /// </summary>
    public string FullName =>
        string.IsNullOrWhiteSpace(LastName)
            ? FirstName
            : $"{FirstName} {LastName}";

    #endregion

    #region Account Status

    /// <summary>
    /// Gets or sets a value indicating whether the user account is active.
    /// </summary>
    public bool IsActive { get; set; } = UserConstant.Defaults.IsActive;

    /// <summary>
    /// Gets or sets the UTC timestamp of the user's last login.
    /// </summary>
    public DateTimeOffset? LastLoginAtUtc { get; set; }

    #endregion

    #region Sign-In Tracking

    /// <summary>
    /// Gets or sets the IP address from the current sign-in session.
    /// </summary>
    public string? CurrentSignInIp { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp of the current sign-in.
    /// </summary>
    public DateTimeOffset? CurrentSignInAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the IP address from the previous sign-in session.
    /// </summary>
    public string? LastSignInIp { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp of the previous sign-in.
    /// </summary>
    public DateTimeOffset? LastSignInAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the cumulative sign-in count for the user.
    /// </summary>
    public int SignInCount { get; set; }

    #endregion

    #region Auditing
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Navigation

    /// <summary>
    /// Gets or sets the collection of refresh tokens issued to the user.
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of custom claims assigned to the user.
    /// </summary>
    public ICollection<UserClaim> Claims { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of roles assigned to the user.
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of external logins for the user.
    /// </summary>
    public ICollection<UserLogin> UserLogins { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of security tokens for the user.
    /// </summary>
    public ICollection<UserToken> UserTokens { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of passkeys for the user.
    /// </summary>
    public ICollection<UserPasskey> Passkeys { get; set; } = [];

    #endregion
}