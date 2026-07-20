using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Shared.Operational.Persistence.Data;
using Shared.Security.Authentication.Tokens.Services.Refresh.Store;
using Shared.Security.Identity.Domain.Tokens;

namespace Shared.UnitTests.Security.Authentication.Tokens.Services.Refresh.Store;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "RefreshTokenStore")]
public sealed class RefreshTokenStoreTests
{
    private readonly Mock<IApplicationDbContext> _dbContextMock = new();
    private readonly Mock<DbSet<RefreshToken>> _dbSetMock;
    private readonly RefreshTokenStore _store;

    public RefreshTokenStoreTests()
    {
        _dbSetMock = CreateDbSetMock(new List<RefreshToken>());
        _dbContextMock.Setup(c => c.Set<RefreshToken>()).Returns(_dbSetMock.Object);
        _store = new RefreshTokenStore(_dbContextMock.Object, Mock.Of<ILogger<RefreshTokenStore>>());
    }

    [Fact(DisplayName = "GetByTokenHashAsync should return null when hash is empty")]
    public async Task GetByTokenHashAsync_ReturnsNull_WhenEmptyHash()
    {
        // Arrange
        string tokenHash = string.Empty;

        // Act
        RefreshToken? result = await _store.GetByTokenHashAsync(tokenHash);

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "AddAsync should persist entity to store")]
    public async Task AddAsync_PersistsEntity()
    {
        // Arrange
        RefreshToken entity = new()
        {
            Id = Guid.NewGuid(),
            TokenHash = "new-token-hash",
            UserId = Guid.NewGuid(),
            TokenFamilyId = Guid.NewGuid(),
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(7)
        };

        // Act
        await _store.AddAsync(entity);

        // Assert
        _dbSetMock.Verify(s => s.Add(It.Is<RefreshToken>(rt => rt.TokenHash == "new-token-hash")), Times.Once);
        _dbContextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "UpdateAsync should call SaveChangesAsync")]
    public async Task UpdateAsync_PersistsChanges()
    {
        // Arrange
        RefreshToken entity = new()
        {
            Id = Guid.NewGuid(),
            TokenHash = "update-token",
            UserId = Guid.NewGuid(),
            TokenFamilyId = Guid.NewGuid(),
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(1)
        };

        // Act
        await _store.UpdateAsync(entity);

        // Assert
        _dbContextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SaveChangesAsync should call DbContext.SaveChangesAsync")]
    public async Task SaveChangesAsync_CallsDbContext()
    {
        // Act
        await _store.SaveChangesAsync();

        // Assert
        _dbContextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Mock<DbSet<T>> CreateDbSetMock<T>(List<T> data) where T : class
    {
        IQueryable<T> queryable = data.AsQueryable();
        Mock<DbSet<T>> dbSetMock = new();

        dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

        return dbSetMock;
    }
}
