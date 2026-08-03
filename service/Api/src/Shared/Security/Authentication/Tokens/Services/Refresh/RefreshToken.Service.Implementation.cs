using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Options;

using Shared.Security.Authentication.Tokens.Models;
using Shared.Security.Authentication.Tokens.Options;
using Shared.Security.Authentication.Tokens.Services.Refresh.Protections;
using Shared.Security.Authentication.Tokens.Services.Refresh.Store;
using Shared.Security.Identity.Domain.Tokens;

namespace Shared.Security.Authentication.Tokens.Services.Refresh;

/// <summary>Issues, validates, rotates, and revokes refresh tokens with optional sliding expiration, theft detection, and blacklist integration.</summary>
// Invariant: TokenHash uniquely identifies each token; one TokenFamilyId per token chain; revoked tokens never become un-revoked.
// Context: Rotation and reuse detection mitigate refresh token theft (Threat TMT-TOK-003, TMT-TOK-004).
public partial class RefreshTokenService(
    IRefreshTokenStore refreshTokenStore,
    ITokenBlacklistService? tokenBlacklistService,
    IOptions<JwtSettings> jwtOptions,
    ITokenTheftDetector? tokenTheftDetector,
    ILogger<RefreshTokenService> logger,
    ICurrentUser? currentUser = null) : IRefreshTokenService
{
    private readonly JwtSettings _jwtOptions = jwtOptions.Value;
    private readonly TokenSecurityOptions _tokenSecurityOptions = jwtOptions.Value.TokenSecurity;

    /// <summary>Issues a new refresh token with cryptographically secure random value and persists to store.</summary>
    // Contract: pre=userId!=Guid.Empty, post=return.IsSuccess && entity.Id!=Guid.Empty, throws=Exception on persistence failure
    public async Task<Result<RefreshTokenResponseModel>> GenerateAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            // Generate: cryptographically strong random token to prevent brute-force guessing (TMT-TOK-003)
            string rawToken = GenerateSecureToken();
            string tokenHash = ComputeSha256Hash(rawToken);

            // Create: refresh token entity bound to user with device fingerprint for audit trail
            var entity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                TokenHash = tokenHash,
                UserId = userId,
                TokenFamilyId = Guid.NewGuid(),
                CreatedAtUtc = DateTimeOffset.UtcNow,
                ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationInDays),
                LastUsedAtUtc = DateTimeOffset.UtcNow,
                DeviceId = currentUser?.SessionId,
                UserAgent = currentUser?.Device,
                IpAddress = currentUser?.IpAddress
            };

            // Call: persist to primary store (module boundary: Service → Store)
            await refreshTokenStore.AddAsync(entity, ct);

            // Log: record successful token issuance for audit trail
            Loggers.LogTokenGenerated(logger, userId, entity.ExpiresAtUtc, _tokenSecurityOptions.RotationEnabled);

            // Transform: domain entity to public response DTO — raw token returned only at issuance
            return MapToResponse(entity, rawToken);
        }
        catch (Exception ex)
        {
            // Catch: token generation failure must not leak cryptographic details to caller
            Loggers.LogTokenGenerationFailed(logger, userId, ex);
            return Result<RefreshTokenResponseModel>.Unexpected(
                exception: ex,
                errors: [RefreshTokenResult.Failure.GenerationFailed]);
        }
    }

    /// <summary>Retrieves a refresh token by raw value, validates expiry and revocation, applies sliding expiration, and checks theft replays.</summary>
    // Contract: pre=token!=null && token.Length>0, post=return.IsSuccess implies entity.IsActive, throws=Exception on store failure
    public async Task<Result<RefreshTokenResponseModel>> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        // Guard: reject empty or whitespace token to avoid unnecessary hash computation
        if (string.IsNullOrWhiteSpace(token))
            return RefreshTokenResult.Failure.NotFound;

        // Compute: SHA256 hash for secure store lookup — raw token never stored or logged
        string tokenHash = ComputeSha256Hash(token);

        // Call: retrieve token by hash from store (module boundary: Service → Store)
        RefreshToken? entity = await refreshTokenStore.GetByTokenHashAsync(tokenHash, ct);

        if (entity is null)
            return RefreshTokenResult.Failure.NotFound;

        // Validate: token must not exceed its configured lifetime
        if (entity.IsExpired)
            return RefreshTokenResult.Failure.Expired;

        // Validate: token must not have been previously revoked
        if (entity.IsRevoked)
            return RefreshTokenResult.Failure.Revoked;

        // Compute: slide expiration forward when enabled and below max-age ceiling to extend valid session
        if (_tokenSecurityOptions.SlidingExpirationEnabled && entity.LastUsedAtUtc.HasValue)
        {
            DateTimeOffset maxAge = DateTime.UtcNow.AddDays(_tokenSecurityOptions.MaxTokenAgeDays);
            if (entity.ExpiresAtUtc < maxAge)
            {
                entity.ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationInDays);
                entity.LastUsedAtUtc = DateTime.UtcNow;
                await refreshTokenStore.UpdateAsync(entity, ct);

                Loggers.LogSlidingExpirationApplied(logger, entity.Id, entity.ExpiresAtUtc);
            }
        }

        // Validate: check for token replay using theft detector — mitigates TMT-TOK-004
        if (_tokenSecurityOptions.ReuseDetectionEnabled && tokenTheftDetector is not null)
        {
            Result<bool> theftResult = await tokenTheftDetector.IsTokenReusedAsync(
                token, entity.UserId, ct);

            if (theftResult.IsFailure)
                return theftResult.Errors;

            if (theftResult.Value)
                return RefreshTokenResult.Failure.TheftDetected;

            // Call: record token usage to detect future replays
            await tokenTheftDetector.MarkTokenAsUsedAsync(
                token, entity.UserId, ct);
        }

        return MapToResponse(entity);
    }

    /// <summary>Revokes a single refresh token by raw value and optionally adds to global blacklist.</summary>
    // Contract: pre=request.Token!=null, post=entity.RevokedAtUtc!=null, throws=Exception on store failure
    public async Task<Result> RevokeAsync(RevokeTokenRequestModel request, CancellationToken ct = default)
    {
        // Guard: reject empty token — prevents unnecessary store lookup
        if (string.IsNullOrEmpty(request.Token))
            return RefreshTokenResult.Failure.NotFound;

        // Compute: hash the raw token for secure lookup
        string tokenHash = ComputeSha256Hash(request.Token);

        // Call: fetch the targeted token from store
        RefreshToken? entity = await refreshTokenStore.GetByTokenHashAsync(tokenHash, ct);

        if (entity is null)
            return RefreshTokenResult.Failure.NotFound;

        // Update: mark token as revoked with reason for audit trail
        entity.RevokedAtUtc = DateTimeOffset.UtcNow;
        entity.RevocationReason = MapRevocationReason(request.Reason);
        await refreshTokenStore.UpdateAsync(entity, ct);

        // Call: optionally add token ID to global blacklist for immediate invalidation (boundary: Service → Blacklist)
        if (tokenBlacklistService is not null)
        {
            await tokenBlacklistService.BlacklistTokenAsync(entity.Id.ToString(), entity.ExpiresAtUtc.UtcDateTime, ct);
        }

        // Log: record security event for audit
        Loggers.LogTokenRevoked(logger, entity.Id, entity.UserId, request.Reason);

        return RefreshTokenResult.Success.Revoked;
    }

    /// <summary>Revokes all active tokens for a user — used during password change, account lock, or theft response.</summary>
    // Contract: pre=userId!=Guid.Empty, post=return.IsSuccess, throws=Exception on store failure
    public async Task<Result<int>> RevokeAllForUserAsync(Guid userId, string reason, CancellationToken ct = default)
    {
        // Call: fetch all active tokens for user (boundary: Service → Store)
        List<RefreshToken> activeTokens = await refreshTokenStore.GetActiveByUserIdAsync(userId, ct);

        if (activeTokens.Count == 0)
            return 0;

        // Update: bulk-revoke all active tokens to invalidate all sessions
        DateTimeOffset now = DateTimeOffset.UtcNow;
        RefreshTokenRevocationReason revocationReason = MapRevocationReason(reason);
        foreach (RefreshToken rt in activeTokens)
        {
            rt.RevokedAtUtc = now;
            rt.RevocationReason = revocationReason;
        }

        // Call: flush all revocations in single transaction (module boundary: Service → Store)
        await refreshTokenStore.SaveChangesAsync(ct);

        // Log: audit record for mass session invalidation
        Loggers.LogAllTokensRevoked(logger, activeTokens.Count, userId, reason);
        return RefreshTokenResult.Success.AllRevoked(activeTokens.Count);
    }

    /// <summary>Exchanges an existing refresh token for a new one in the same family, revoking the old token — optionally detects theft of already-rotated tokens.</summary>
    // Contract: pre=token!=null, post=return.IsSuccess implies oldEntity.RevokedAtUtc!=null && newEntity.Id!=null, throws=Exception on persistence failure
    public async Task<Result<RefreshTokenResponseModel>> RotateAsync(string token, CancellationToken ct = default)
    {
        // Compute: hash the raw token for store lookup
        string tokenHash = ComputeSha256Hash(token);

        // Call: fetch the existing token from store
        RefreshToken? oldEntity = await refreshTokenStore.GetByTokenHashAsync(tokenHash, ct);

        // Validate: token must exist and be active for rotation
        if (oldEntity is null)
            return RefreshTokenResult.Failure.NotFound;

        // Validate: expired tokens cannot be rotated
        if (oldEntity.IsExpired)
            return RefreshTokenResult.Failure.Expired;

        // Validate: already-revoked token indicates potential theft (TMT-TOK-004)
        if (oldEntity.IsRevoked)
        {
            // Recover: revoke all user tokens as safety measure when reuse detected
            if (_tokenSecurityOptions.ReuseDetectionEnabled)
            {
                await RevokeAllForUserAsync(oldEntity.UserId, RefreshTokenConstant.RevocationReasons.ReuseDetected, ct);
                return RefreshTokenResult.Failure.TheftDetected;
            }

            return RefreshTokenResult.Failure.Revoked;
        }

        // AgentHint: When rotation is disabled, returning existing token avoids unnecessary churn
        if (!_tokenSecurityOptions.RotationEnabled)
        {
            return MapToResponse(oldEntity);
        }

        try
        {
            // Generate: cryptographically strong new token for rotation
            string newRawToken = GenerateSecureToken();
            string newTokenHash = ComputeSha256Hash(newRawToken);

            // Create: new entity shares TokenFamilyId with old token to preserve rotation chain
            RefreshToken newEntity = new()
            {
                Id = Guid.NewGuid(),
                TokenHash = newTokenHash,
                UserId = oldEntity.UserId,
                TokenFamilyId = oldEntity.TokenFamilyId,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationInDays),
                LastUsedAtUtc = DateTimeOffset.UtcNow,
                DeviceId = oldEntity.DeviceId,
                UserAgent = oldEntity.UserAgent,
                IpAddress = oldEntity.IpAddress
            };

            // Update: revoke old token and link rotation chain for audit traceability
            oldEntity.RevokedAtUtc = DateTimeOffset.UtcNow;
            oldEntity.RevocationReason = RefreshTokenRevocationReason.Replaced;
            oldEntity.ReplacedByTokenId = newEntity.Id;

            // Call: persist new token first, then update old — order prevents orphan detection gap
            await refreshTokenStore.AddAsync(newEntity, ct);
            await refreshTokenStore.UpdateAsync(oldEntity, ct);

            // Log: record rotation event for audit trail
            Loggers.LogTokenRotated(logger, oldEntity.UserId, oldEntity.Id, newEntity.Id);
            return MapToResponse(newEntity, newRawToken);
        }
        catch (Exception ex)
        {
            // Catch: rotation failure must not orphan old token — caller may retry
            Loggers.LogTokenRotationFailed(logger, oldEntity.UserId, ex);
            return Result<RefreshTokenResponseModel>.Unexpected(
                exception: ex,
                errors: [RefreshTokenResult.Failure.RotationFailed]);
        }
    }

    private static string GenerateSecureToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static string ComputeSha256Hash(string rawData)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes);
    }

    private static RefreshTokenResponseModel MapToResponse(RefreshToken entity, string? rawToken = null)
    {
        return new RefreshTokenResponseModel
        {
            Id = entity.Id,
            Token = rawToken ?? string.Empty,
            UserId = entity.UserId,
            CreatedAt = entity.CreatedAtUtc.UtcDateTime,
            ExpiresAt = entity.ExpiresAtUtc.UtcDateTime,
            RevokedAt = entity.RevokedAtUtc?.UtcDateTime,
            RevokedReason = entity.RevocationReason?.ToString(),
            ReplacedByToken = entity.ReplacedByTokenId?.ToString(),
            IsActive = entity.IsActive
        };
    }

    private static RefreshTokenRevocationReason MapRevocationReason(string? reason)
    {
        // Compute: Map string representation to domain enum
        return reason?.ToLowerInvariant() switch
        {
            RefreshTokenConstant.RevocationReasons.Replaced => RefreshTokenRevocationReason.Replaced,
            RefreshTokenConstant.RevocationReasons.UserLogout => RefreshTokenRevocationReason.UserLogout,
            RefreshTokenConstant.RevocationReasons.UserLogoutAll => RefreshTokenRevocationReason.UserLogoutAll,
            RefreshTokenConstant.RevocationReasons.ReuseDetected => RefreshTokenRevocationReason.ReuseDetected,
            _ => RefreshTokenRevocationReason.UserLogout
        };
    }
}
