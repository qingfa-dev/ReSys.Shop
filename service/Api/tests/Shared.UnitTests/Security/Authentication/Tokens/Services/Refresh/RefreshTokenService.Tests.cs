using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Shared.Security.Authentication.Tokens.Models;
using Shared.Security.Authentication.Tokens.Options;
using Shared.Security.Authentication.Tokens.Services.Refresh;
using Shared.Security.Authentication.Tokens.Services.Refresh.Protections;
using Shared.Security.Authentication.Tokens.Services.Refresh.Store;
using Shared.Security.Identity.Domain.Tokens;

namespace Shared.UnitTests.Security.Authentication.Tokens.Services.Refresh;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "RefreshTokenService")]
public sealed class RefreshTokenServiceTests
{
    private readonly Mock<IRefreshTokenStore> _storeMock = new();
    private readonly JwtSettings _jwtSettings = new()
    {
        Secret = "super-secret-key-that-is-long-enough-for-testing-123!",
        Issuer = "test-issuer",
        Audience = "test-audience",
        AccessTokenExpirationInMinutes = 15,
        RefreshTokenExpirationInDays = 7,
        TokenSecurity = new TokenSecurityOptions
        {
            RotationEnabled = true,
            ReuseDetectionEnabled = true,
            SlidingExpirationEnabled = true,
            MaxTokenAgeDays = 30
        }
    };

    private RefreshTokenService CreateService(
        ITokenBlacklistService? blacklist = null,
        ITokenTheftDetector? theftDetector = null)
    {
        IOptions<JwtSettings> options = Microsoft.Extensions.Options.Options.Create(_jwtSettings);
        return new RefreshTokenService(
            _storeMock.Object,
            blacklist,
            options,
            theftDetector,
            Mock.Of<ILogger<RefreshTokenService>>());
    }

    private static RefreshToken CreateActiveToken()
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = "hash",
            UserId = Guid.NewGuid(),
            TokenFamilyId = Guid.NewGuid(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(7),
            LastUsedAtUtc = DateTimeOffset.UtcNow,
            DeviceId = "session-1",
            UserAgent = "Mozilla/5.0",
            IpAddress = "127.0.0.1"
        };
    }

    [Fact(DisplayName = "GenerateAsync should return token response on success")]
    public async Task GenerateAsync_ReturnsTokenResponse()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RefreshTokenService service = CreateService();

        // Act
        Result<RefreshTokenResponseModel> result = await service.GenerateAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Token.Should().NotBeEmpty();
        result.Value.UserId.Should().Be(userId);
    }

    [Fact(DisplayName = "GenerateAsync should return failure on store error")]
    public async Task GenerateAsync_ReturnsFailure_OnStoreError()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _storeMock.Setup(s => s.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));
        RefreshTokenService service = CreateService();

        // Act
        Result<RefreshTokenResponseModel> result = await service.GenerateAsync(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "RefreshToken.GenerationFailed");
    }

    [Fact(DisplayName = "GetByTokenAsync should return token when valid and active")]
    public async Task GetByTokenAsync_ReturnsToken_WhenValid()
    {
        // Arrange
        RefreshToken entity = CreateActiveToken();
        _storeMock.Setup(s => s.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        RefreshTokenService service = CreateService();

        // Act
        Result<RefreshTokenResponseModel> result = await service.GetByTokenAsync("valid-token");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(entity.UserId);
    }

    [Fact(DisplayName = "GetByTokenAsync should return not found when token is null")]
    public async Task GetByTokenAsync_ReturnsNotFound_WhenNullToken()
    {
        // Arrange
        RefreshTokenService service = CreateService();

        // Act
        Result<RefreshTokenResponseModel> result = await service.GetByTokenAsync(string.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "RefreshToken.NotFound");
    }

    [Fact(DisplayName = "GetByTokenAsync should return expired when token is expired")]
    public async Task GetByTokenAsync_ReturnsExpired_WhenExpired()
    {
        // Arrange
        RefreshToken entity = CreateActiveToken();
        entity.ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(-1);
        _storeMock.Setup(s => s.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        RefreshTokenService service = CreateService();

        // Act
        Result<RefreshTokenResponseModel> result = await service.GetByTokenAsync("expired-token");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "RefreshToken.Expired");
    }

    [Fact(DisplayName = "GetByTokenAsync should return revoked when token is revoked")]
    public async Task GetByTokenAsync_ReturnsRevoked_WhenRevoked()
    {
        // Arrange
        RefreshToken entity = CreateActiveToken();
        entity.RevokedAtUtc = DateTimeOffset.UtcNow.AddDays(-1);
        _storeMock.Setup(s => s.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        RefreshTokenService service = CreateService();

        // Act
        Result<RefreshTokenResponseModel> result = await service.GetByTokenAsync("revoked-token");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "RefreshToken.Revoked");
    }

    [Fact(DisplayName = "RevokeAsync should revoke token successfully")]
    public async Task RevokeAsync_RevokesToken()
    {
        // Arrange
        RefreshToken entity = CreateActiveToken();
        _storeMock.Setup(s => s.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        RefreshTokenService service = CreateService();

        RevokeTokenRequestModel request = new() { Token = "token-to-revoke", Reason = "user_logout" };

        // Act
        Result result = await service.RevokeAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _storeMock.Verify(s => s.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "RevokeAsync should return not found when token is empty")]
    public async Task RevokeAsync_ReturnsNotFound_WhenTokenEmpty()
    {
        // Arrange
        RefreshTokenService service = CreateService();
        RevokeTokenRequestModel request = new() { Token = string.Empty };

        // Act
        Result result = await service.RevokeAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "RefreshToken.NotFound");
    }

    [Fact(DisplayName = "RevokeAllForUserAsync should revoke all active tokens")]
    public async Task RevokeAllForUserAsync_RevokesAllActive()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        List<RefreshToken> activeTokens = new()
        {
            CreateActiveToken(),
            CreateActiveToken()
        };
        activeTokens[0].UserId = userId;
        activeTokens[1].UserId = userId;

        _storeMock.Setup(s => s.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeTokens);
        RefreshTokenService service = CreateService();

        // Act
        Result<int> result = await service.RevokeAllForUserAsync(userId, "user_logout");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
        _storeMock.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "RotateAsync should create new token and revoke old when rotation enabled")]
    public async Task RotateAsync_RotatesToken_WhenRotationEnabled()
    {
        // Arrange
        RefreshToken oldEntity = CreateActiveToken();
        _storeMock.Setup(s => s.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldEntity);
        RefreshTokenService service = CreateService();

        // Act
        Result<RefreshTokenResponseModel> result = await service.RotateAsync("old-token");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().NotBeEmpty();
        _storeMock.Verify(s => s.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "RotateAsync should detect reuse when revoked token is used")]
    public async Task RotateAsync_DetectsReuse_WhenRevokedTokenUsed()
    {
        // Arrange
        RefreshToken revokedEntity = CreateActiveToken();
        revokedEntity.RevokedAtUtc = DateTimeOffset.UtcNow.AddDays(-1);
        _storeMock.Setup(s => s.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(revokedEntity);
        _storeMock.Setup(s => s.GetActiveByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken>());
        RefreshTokenService service = CreateService();

        // Act
        Result<RefreshTokenResponseModel> result = await service.RotateAsync("reused-token");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "RefreshToken.TheftDetected");
    }

    [Fact(DisplayName = "RotateAsync should return not found when token does not exist")]
    public async Task RotateAsync_ReturnsNotFound_WhenTokenNotFound()
    {
        // Arrange
        _storeMock.Setup(s => s.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);
        RefreshTokenService service = CreateService();

        // Act
        Result<RefreshTokenResponseModel> result = await service.RotateAsync("nonexistent");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "RefreshToken.NotFound");
    }
}
