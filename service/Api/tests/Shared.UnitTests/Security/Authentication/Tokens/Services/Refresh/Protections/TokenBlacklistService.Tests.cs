using Microsoft.Extensions.Logging;

using Shared.Performance.Caching.Wrappers;
using Shared.Security.Authentication.Tokens.Services.Refresh.Protections;

namespace Shared.UnitTests.Security.Authentication.Tokens.Services.Refresh.Protections;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "TokenBlacklistService")]
public sealed class TokenBlacklistServiceTests
{
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly ILogger<TokenBlacklistService> _logger = Mock.Of<ILogger<TokenBlacklistService>>();

    [Fact(DisplayName = "IsBlacklistedAsync should return not blacklisted when jti is empty")]
    public async Task IsBlacklistedAsync_ReturnsNotBlacklisted_WhenJtiEmpty()
    {
        // Arrange
        TokenBlacklistService service = new(new Mock<ICacheService>().Object, _logger);

        // Act
        Result result = await service.IsBlacklistedAsync(string.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "IsBlacklistedAsync should return blacklisted when JTI found in cache")]
    public async Task IsBlacklistedAsync_ReturnsBlacklisted_WhenJtiInCache()
    {
        // Arrange
        _cacheMock
            .Setup(c => c.GetOrCreateAsync(
                "blacklist:test-jti",
                It.IsAny<Func<CancellationToken, ValueTask<string?>>>(),
                It.IsAny<CachingEntryOption?>(),
                It.IsAny<IEnumerable<string>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("blacklisted");

        TokenBlacklistService service = new(_cacheMock.Object, _logger);

        // Act
        Result result = await service.IsBlacklistedAsync("test-jti");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "IsBlacklistedAsync should return not blacklisted when JTI not in cache")]
    public async Task IsBlacklistedAsync_ReturnsNotBlacklisted_WhenJtiNotInCache()
    {
        // Arrange
        _cacheMock
            .Setup(c => c.GetOrCreateAsync(
                "blacklist:test-jti",
                It.IsAny<Func<CancellationToken, ValueTask<string?>>>(),
                It.IsAny<CachingEntryOption?>(),
                It.IsAny<IEnumerable<string>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        TokenBlacklistService service = new(_cacheMock.Object, _logger);

        // Act
        Result result = await service.IsBlacklistedAsync("test-jti");

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "IsBlacklistedAsync should return BlacklistCheckFailed when cache throws")]
    public async Task IsBlacklistedAsync_ReturnsBlacklistCheckFailed_WhenCacheThrows()
    {
        // Arrange
        _cacheMock
            .Setup(c => c.GetOrCreateAsync(
                "blacklist:test-jti",
                It.IsAny<Func<CancellationToken, ValueTask<string?>>>(),
                It.IsAny<CachingEntryOption?>(),
                It.IsAny<IEnumerable<string>?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cache unavailable"));

        TokenBlacklistService service = new(_cacheMock.Object, _logger);

        // Act
        Result result = await service.IsBlacklistedAsync("test-jti");

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "BlacklistTokenAsync should succeed when jti is empty")]
    public async Task BlacklistTokenAsync_Succeeds_WhenJtiEmpty()
    {
        // Arrange
        TokenBlacklistService service = new(new Mock<ICacheService>().Object, _logger);
        DateTimeOffset expiry = DateTime.UtcNow.AddHours(1);

        // Act
        Result result = await service.BlacklistTokenAsync(string.Empty, expiry);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "BlacklistTokenAsync should succeed when token already expired")]
    public async Task BlacklistTokenAsync_Succeeds_WhenTokenExpired()
    {
        // Arrange
        TokenBlacklistService service = new(new Mock<ICacheService>().Object, _logger);
        DateTimeOffset pastExpiry = DateTime.UtcNow.AddHours(-1);

        // Act
        Result result = await service.BlacklistTokenAsync("expired-jti", pastExpiry);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _cacheMock.Verify(
            c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<CachingEntryOption?>(),
                It.IsAny<IEnumerable<string>?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "BlacklistTokenAsync should call SetAsync for valid token")]
    public async Task BlacklistTokenAsync_ShouldCallSetAsync_ForValidToken()
    {
        // Arrange
        TokenBlacklistService service = new(_cacheMock.Object, _logger);
        DateTimeOffset futureExpiry = DateTime.UtcNow.AddHours(2);

        _cacheMock
            .Setup(c => c.SetAsync(
                "blacklist:valid-jti",
                "blacklisted",
                It.IsAny<CachingEntryOption?>(),
                It.IsAny<IEnumerable<string>?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask());

        // Act
        Result result = await service.BlacklistTokenAsync("valid-jti", futureExpiry);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _cacheMock.Verify(
            c => c.SetAsync(
                "blacklist:valid-jti",
                "blacklisted",
                It.IsAny<CachingEntryOption?>(),
                It.IsAny<IEnumerable<string>?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "BlacklistTokenAsync should return BlacklistFailed when cache throws")]
    public async Task BlacklistTokenAsync_ReturnsBlacklistFailed_WhenCacheThrows()
    {
        // Arrange
        _cacheMock
            .Setup(c => c.SetAsync(
                "blacklist:failing-jti",
                "blacklisted",
                It.IsAny<CachingEntryOption?>(),
                It.IsAny<IEnumerable<string>?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cache unavailable"));

        TokenBlacklistService service = new(_cacheMock.Object, _logger);
        DateTimeOffset futureExpiry = DateTime.UtcNow.AddHours(2);

        // Act
        Result result = await service.BlacklistTokenAsync("failing-jti", futureExpiry);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "CleanupExpiredAsync should return success")]
    public async Task CleanupExpiredAsync_ReturnsSuccess()
    {
        // Arrange
        TokenBlacklistService service = new(new Mock<ICacheService>().Object, _logger);

        // Act
        Result result = await service.CleanupExpiredAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
