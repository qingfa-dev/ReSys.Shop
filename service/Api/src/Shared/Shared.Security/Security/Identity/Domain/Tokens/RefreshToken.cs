using Shared.Application.Domain.Models;
using Shared.Security.Identity.Domain.Users;

namespace Shared.Security.Identity.Domain.Tokens;
public class RefreshToken : Entity
{
    /// <summary>
    /// SHA256 hash of refresh token.
    /// Never store raw tokens.
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    /// <summary>
    /// Shared by all rotated tokens originating from the same login.
    /// </summary>
    public Guid TokenFamilyId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } =
        DateTimeOffset.UtcNow;

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public DateTimeOffset? LastUsedAtUtc { get; set; }

    public DateTimeOffset? RevokedAtUtc { get; set; }

    public RefreshTokenRevocationReason? RevocationReason { get; set; }

    /// <summary>
    /// Rotation chain.
    /// </summary>
    public Guid? ReplacedByTokenId { get; set; }

    public RefreshToken? ReplacedByToken { get; set; }

    public string? DeviceId { get; set; }

    public string? UserAgent { get; set; }

    public string? IpAddress { get; set; }

    public bool IsExpired =>
        DateTimeOffset.UtcNow >= ExpiresAtUtc;

    public bool IsRevoked =>
        RevokedAtUtc.HasValue;

    public bool IsActive =>
        !IsExpired && !IsRevoked;
}