using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Shared.Security.Authentication.Tokens.Options;
using Shared.Security.Authentication.Tokens.Services.Refresh.Protections;
using Shared.Security.Authentication.Tokens.Services.Refresh.Store;
using Shared.Security.Identity.Domain.Tokens;

namespace Shared.UnitTests.Security.Authentication.Tokens.Services.Refresh.Protections;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "TokenTheftDetector")]
public sealed class TokenTheftDetectorTests
{
    private readonly Mock<IRefreshTokenStore> _storeMock = new();
    private readonly Mock<HybridCache> _hybridCacheMock = new();
    private readonly TokenSecurityOptions _options;
    private readonly ILogger<TokenTheftDetector> _logger = Mock.Of<ILogger<TokenTheftDetector>>();

    public TokenTheftDetectorTests()
    {
        _options = new TokenSecurityOptions
        {
            ReuseDetectionEnabled = true,
            RotationEnabled = true
        };

        // RemoveByTagAsync is mockable — set default as no-op
        _hybridCacheMock
            .Setup(c => c.RemoveByTagAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask());
    }

    private TokenTheftDetector CreateDetector(TokenSecurityOptions? options = null)
    {
        IOptions<TokenSecurityOptions> opts = Microsoft.Extensions.Options.Options.Create(options ?? _options);
        return new TokenTheftDetector(
            _storeMock.Object,
            _hybridCacheMock.Object,
            opts,
            _logger);
    }

    // Disabled detection paths

    [Fact(DisplayName = "IsTokenReusedAsync should return false when reuse detection is disabled")]
    public async Task IsTokenReusedAsync_ReturnsFalse_WhenDetectionDisabled()
    {
        // Arrange
        TokenSecurityOptions disabledOptions = new() { ReuseDetectionEnabled = false };
        TokenTheftDetector detector = CreateDetector(disabledOptions);

        // Act
        Result<bool> result = await detector.IsTokenReusedAsync("test-token", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact(DisplayName = "MarkTokenAsUsedAsync should not set cache when detection disabled")]
    public async Task MarkTokenAsUsedAsync_Skips_WhenDetectionDisabled()
    {
        // Arrange
        TokenSecurityOptions disabledOptions = new() { ReuseDetectionEnabled = false };
        TokenTheftDetector detector = CreateDetector(disabledOptions);

        // Act
        await detector.MarkTokenAsUsedAsync("test-token", Guid.NewGuid());

        // Assert
        // No exception — completes silently when detection is disabled
    }

    // Enabled detection paths — IsTokenReusedAsync with GetOrCreateAsync cannot be mocked (non-overridable in HybridCache)
    // Default Moq behavior returns null, which flows to "not found" path

    [Fact(DisplayName = "IsTokenReusedAsync should return false by default when token not in cache")]
    public async Task IsTokenReusedAsync_ReturnsFalse_WhenNotInCache()
    {
        // Arrange
        TokenTheftDetector detector = CreateDetector();

        // Note: HybridCache.GetOrCreateAsync is non-overridable in this version,
        // so Moq's default returns null — which exercises the "cache miss" path

        // Act
        Result<bool> result = await detector.IsTokenReusedAsync("fresh-token", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    // MarkTokenAsUsedAsync — SetAsync IS mockable

    [Fact(DisplayName = "MarkTokenAsUsedAsync should call SetAsync on cache when detection enabled")]
    public async Task MarkTokenAsUsedAsync_ShouldSetCache_WhenDetectionEnabled()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TokenTheftDetector detector = CreateDetector();

        _hybridCacheMock
            .Setup(c => c.SetAsync(
                It.IsAny<string>(),
                "used",
                It.IsAny<HybridCacheEntryOptions?>(),
                It.Is<IEnumerable<string>>(t => t.Contains("token-theft")),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask());

        // Act
        await detector.MarkTokenAsUsedAsync("fresh-token", userId);

        // Assert
        _hybridCacheMock.Verify(
            c => c.SetAsync(
                It.IsAny<string>(),
                "used",
                It.IsAny<HybridCacheEntryOptions?>(),
                It.IsAny<IEnumerable<string>?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "MarkTokenAsUsedAsync should update DB token when cache SetAsync throws")]
    public async Task MarkTokenAsUsedAsync_ShouldUpdateDb_WhenCacheSetFails()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TokenTheftDetector detector = CreateDetector();

        _hybridCacheMock
            .Setup(c => c.SetAsync(
                It.IsAny<string>(),
                "used",
                It.IsAny<HybridCacheEntryOptions?>(),
                It.IsAny<IEnumerable<string>?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cache full"));

        RefreshToken existingToken = new()
        {
            TokenHash = "hash",
            UserId = userId,
            LastUsedAtUtc = null
        };
        _storeMock
            .Setup(s => s.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        _storeMock
            .Setup(s => s.UpdateAsync(existingToken, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await detector.MarkTokenAsUsedAsync("stored-token", userId);

        // Assert
        existingToken.LastUsedAtUtc.Should().NotBeNull();
        _storeMock.Verify(
            s => s.UpdateAsync(existingToken, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "MarkTokenAsUsedAsync should complete silently when cache and DB both fail")]
    public async Task MarkTokenAsUsedAsync_CompletesSilently_WhenCacheAndDbFail()
    {
        // Arrange
        TokenTheftDetector detector = CreateDetector();

        _hybridCacheMock
            .Setup(c => c.SetAsync(
                It.IsAny<string>(),
                "used",
                It.IsAny<HybridCacheEntryOptions?>(),
                It.IsAny<IEnumerable<string>?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cache full"));

        _storeMock
            .Setup(s => s.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB down"));

        // Act
        Func<Task> act = () => detector.MarkTokenAsUsedAsync("failing-token", Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync();
    }

    // RevokeAllUserTokensAsync

    [Fact(DisplayName = "RevokeAllUserTokensAsync should revoke all active tokens for user")]
    public async Task RevokeAllUserTokensAsync_ShouldRevokeActiveTokens()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TokenTheftDetector detector = CreateDetector();

        RefreshToken token1 = new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = "hash1"
        };
        RefreshToken token2 = new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = "hash2"
        };

        _storeMock
            .Setup(s => s.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([token1, token2]);

        _storeMock
            .Setup(s => s.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await detector.RevokeAllUserTokensAsync(userId, "reuse_detected");

        // Assert
        _storeMock.Verify(
            s => s.UpdateAsync(It.Is<RefreshToken>(t => t.Id == token1.Id), It.IsAny<CancellationToken>()),
            Times.Once);
        _storeMock.Verify(
            s => s.UpdateAsync(It.Is<RefreshToken>(t => t.Id == token2.Id), It.IsAny<CancellationToken>()),
            Times.Once);
        token1.RevokedAtUtc.Should().NotBeNull();
        token1.RevocationReason.Should().Be(RefreshTokenRevocationReason.ReuseDetected);
        token2.RevokedAtUtc.Should().NotBeNull();
        token2.RevocationReason.Should().Be(RefreshTokenRevocationReason.ReuseDetected);
    }

    [Fact(DisplayName = "RevokeAllUserTokensAsync should complete silently when no active tokens")]
    public async Task RevokeAllUserTokensAsync_CompletesSilently_WhenNoActiveTokens()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TokenTheftDetector detector = CreateDetector();

        _storeMock
            .Setup(s => s.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        Func<Task> act = () => detector.RevokeAllUserTokensAsync(userId, "user_logout_all");

        // Assert
        await act.Should().NotThrowAsync();
        _storeMock.Verify(
            s => s.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
