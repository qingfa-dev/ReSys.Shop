using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Options;

using Shared.Security.Authentication.Tokens.Models;
using Shared.Security.Authentication.Tokens.Options;
using Shared.Security.Authentication.Tokens.Services.Refresh.Protections;
using Shared.Security.Authentication.Tokens.Services.Refresh.Store;
using Shared.Security.Identity.Domain.Tokens;

namespace Shared.Security.Authentication.Tokens.Services.Refresh;

/// <summary>
/// Service for managing refresh tokens including generation, retrieval, and revocation.
/// </summary>
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

    /// <inheritdoc/>
    public async Task<Result<RefreshTokenResponseModel>> GenerateAsync(Guid userId, CancellationToken ct = default)
    {
        // Contract: Returns a newly issued refresh token with raw value and entity persisted
        try
        {
            // Generate: Create cryptographically secure random token
            string rawToken = GenerateSecureToken();
            string tokenHash = ComputeSha256Hash(rawToken);

            // Create: Instantiate a new refresh token entity
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

            // Persist: Save to primary store
            await refreshTokenStore.AddAsync(entity, ct);

            // Log: Record successful token issuance
            Loggers.LogTokenGenerated(logger, userId, entity.ExpiresAtUtc, _tokenSecurityOptions.RotationEnabled);

            // Transform: Map to public response DTO
            return MapToResponse(entity, rawToken);
        }
        catch (Exception ex)
        {
            // Log: Detailed error for token generation failure
            Loggers.LogTokenGenerationFailed(logger, userId, ex);
            return RefreshTokenResult.Failure.GenerationFailed;
        }
    }

    /// <inheritdoc/>
    public async Task<Result<RefreshTokenResponseModel>> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        // Validate: Guard against empty or missing token
        if (string.IsNullOrWhiteSpace(token))
            return RefreshTokenResult.Failure.NotFound;

        // Compute: Hash the raw token for secure lookup
        string tokenHash = ComputeSha256Hash(token);

        // Call: Retrieve token data from the store
        RefreshToken? entity = await refreshTokenStore.GetByTokenHashAsync(tokenHash, ct);

        if (entity is null)
            return RefreshTokenResult.Failure.NotFound;

        // Check: Verify token has not reached its expiration date
        if (entity.IsExpired)
            return RefreshTokenResult.Failure.Expired;

        // Check: Verify token has not been manually revoked
        if (entity.IsRevoked)
            return RefreshTokenResult.Failure.Revoked;

        // Update: Apply sliding expiration if enabled and within configured limits
        if (_tokenSecurityOptions.SlidingExpirationEnabled && entity.LastUsedAtUtc.HasValue)
        {
            DateTime maxAge = DateTime.UtcNow.AddDays(_tokenSecurityOptions.MaxTokenAgeDays);
            if (entity.ExpiresAtUtc < maxAge)
            {
                entity.ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationInDays);
                entity.LastUsedAtUtc = DateTime.UtcNow;
                await refreshTokenStore.UpdateAsync(entity, ct);

                Loggers.LogSlidingExpirationApplied(logger, entity.Id, entity.ExpiresAtUtc);
            }
        }

        // Check: Execute theft detection logic for rotated tokens
        if (_tokenSecurityOptions.ReuseDetectionEnabled && tokenTheftDetector is not null)
        {
            Result<bool> theftResult = await tokenTheftDetector.IsTokenReusedAsync(
                token, entity.UserId, ct);

            if (theftResult.IsFailure)
                return theftResult.Errors;

            if (theftResult.Value)
                return RefreshTokenResult.Failure.TheftDetected;

            // Call: Record token usage to detect future replays
            await tokenTheftDetector.MarkTokenAsUsedAsync(
                token, entity.UserId, ct);
        }

        return MapToResponse(entity);
    }

    /// <inheritdoc/>
    public async Task<Result> RevokeAsync(RevokeTokenRequestModel request, CancellationToken ct = default)
    {
        // Validate: Ensure a specific token was targeted for revocation
        if (string.IsNullOrEmpty(request.Token))
            return RefreshTokenResult.Failure.NotFound;

        // Compute: Hash the raw token
        string tokenHash = ComputeSha256Hash(request.Token);

        // Call: Fetch the targeted token from store
        RefreshToken? entity = await refreshTokenStore.GetByTokenHashAsync(tokenHash, ct);

        if (entity is null)
            return RefreshTokenResult.Failure.NotFound;

        // Update: Mark the token as revoked with the provided reason
        entity.RevokedAtUtc = DateTimeOffset.UtcNow;
        entity.RevocationReason = MapRevocationReason(request.Reason);
        await refreshTokenStore.UpdateAsync(entity, ct);

        // Call: Optionally add the token ID to the global blacklist
        if (tokenBlacklistService is not null)
        {
            await tokenBlacklistService.BlacklistTokenAsync(entity.Id.ToString(), entity.ExpiresAtUtc.UtcDateTime, ct);
        }

        // Log: Record security event
        Loggers.LogTokenRevoked(logger, entity.Id, entity.UserId, request.Reason);

        return RefreshTokenResult.Success.Revoked;
    }

    /// <inheritdoc/>
    public async Task<Result<int>> RevokeAllForUserAsync(Guid userId, string reason, CancellationToken ct = default)
    {
        // Contract: All active tokens for the user are revoked and persisted
        // Call: Retrieve all active tokens for the specified user
        List<RefreshToken> activeTokens = await refreshTokenStore.GetActiveByUserIdAsync(userId, ct);

        if (activeTokens.Count == 0)
            return 0;

        // Update: Mark each active token as revoked
        DateTimeOffset now = DateTimeOffset.UtcNow;
        RefreshTokenRevocationReason revocationReason = MapRevocationReason(reason);
        foreach (RefreshToken rt in activeTokens)
        {
            rt.RevokedAtUtc = now;
            rt.RevocationReason = revocationReason;
        }

        // Persist: Flush all revocations in a single transaction
        await refreshTokenStore.SaveChangesAsync(ct);

        // Log: Audit record for mass session invalidation
        Loggers.LogAllTokensRevoked(logger, activeTokens.Count, userId, reason);
        return RefreshTokenResult.Success.AllRevoked(activeTokens.Count);
    }

    /// <inheritdoc/>
    public async Task<Result<RefreshTokenResponseModel>> RotateAsync(string token, CancellationToken ct = default)
    {
        // Validate: Resolve and validate the existing token
        string tokenHash = ComputeSha256Hash(token);

        // Call: Fetch the existing token from store
        RefreshToken? oldEntity = await refreshTokenStore.GetByTokenHashAsync(tokenHash, ct);

        // Check: Token must exist
        if (oldEntity is null)
            return RefreshTokenResult.Failure.NotFound;

        // Check: Token must not be expired
        if (oldEntity.IsExpired)
            return RefreshTokenResult.Failure.Expired;

        // Check: Token reuse detection — already-revoked token indicates potential theft
        if (oldEntity.IsRevoked)
        {
            // Recover: If reuse detection is enabled, revoke all user tokens as safety measure
            if (_tokenSecurityOptions.ReuseDetectionEnabled)
            {
                await RevokeAllForUserAsync(oldEntity.UserId, RefreshTokenConstant.RevocationReasons.ReuseDetected, ct);
                return RefreshTokenResult.Failure.TheftDetected;
            }

            return RefreshTokenResult.Failure.Revoked;
        }

        // AgentHint: When rotation is disabled, return the existing token without issuing a new one
        if (!_tokenSecurityOptions.RotationEnabled)
        {
            return MapToResponse(oldEntity);
        }

        // Handle: Perform token rotation (revoke old, issue new)
        try
        {
            // Generate: Create new cryptographically secure token
            string newRawToken = GenerateSecureToken();
            string newTokenHash = ComputeSha256Hash(newRawToken);

            // Create: Instantiate new token entity with same family and device context
            RefreshToken newEntity = new RefreshToken
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

            // Update: Revoke old token and link rotation chain
            oldEntity.RevokedAtUtc = DateTimeOffset.UtcNow;
            oldEntity.RevocationReason = RefreshTokenRevocationReason.Replaced;
            oldEntity.ReplacedByTokenId = newEntity.Id;

            // Persist: Commit new entity first, then update old entity
            await refreshTokenStore.AddAsync(newEntity, ct);
            await refreshTokenStore.UpdateAsync(oldEntity, ct);

            // Log: Record rotation event for audit trail
            Loggers.LogTokenRotated(logger, oldEntity.UserId, oldEntity.Id, newEntity.Id);
            return MapToResponse(newEntity, newRawToken);
        }
        catch (Exception ex)
        {
            // Log: Detailed error for rotation failure
            Loggers.LogTokenRotationFailed(logger, oldEntity.UserId, ex);
            return RefreshTokenResult.Failure.RotationFailed;
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
        return new RefreshTokenResponseModel(
            Id: entity.Id,
            Token: rawToken ?? string.Empty,
            UserId: entity.UserId,
            CreatedAt: entity.CreatedAtUtc.UtcDateTime,
            ExpiresAt: entity.ExpiresAtUtc.UtcDateTime,
            RevokedAt: entity.RevokedAtUtc?.UtcDateTime,
            RevokedReason: entity.RevocationReason?.ToString(),
            ReplacedByToken: entity.ReplacedByTokenId?.ToString(),
            IsActive: entity.IsActive
        );
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
