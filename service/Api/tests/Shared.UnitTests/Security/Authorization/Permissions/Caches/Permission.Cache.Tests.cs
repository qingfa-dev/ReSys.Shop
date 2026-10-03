using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Shared.Performance.Caching.Wrappers;
using Shared.Security.Authorization.Options;
using Shared.Security.Authorization.Permissions.Caches;

namespace Shared.UnitTests.Security.Authorization.Permissions.Caches;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "PermissionCache")]
public sealed class PermissionCacheTests
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IOptions<AuthzSetting>> _authzOptionsMock;
    private readonly PermissionCache _sut;
    private readonly AuthzSetting _authzSetting;

    public PermissionCacheTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        _authzOptionsMock = new Mock<IOptions<AuthzSetting>>();
        Mock<ILogger<PermissionCache>> loggerMock = new();

        _authzSetting = new AuthzSetting
        {
            PermissionCache = new PermissionCacheOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(5),
                AbsoluteExpiration = TimeSpan.FromMinutes(30),
            }
        };

        _authzOptionsMock.Setup(x => x.Value).Returns(_authzSetting);

        _sut = new PermissionCache(
            _cacheServiceMock.Object,
            _authzOptionsMock.Object,
            loggerMock.Object);
    }

    [Fact(DisplayName = "PermissionCache: GetAsync should return cached permissions on hit")]
    public async Task GetAsync_ShouldReturnCachedPermissions_WhenCacheHit()
    {
        Guid userId = Guid.NewGuid();
        var expectedPerms = new HashSet<string> { "perm1", "perm2" };
        string expectedKey = $"perm:user:{userId}";

        _cacheServiceMock
            .Setup(x => x.GetOrCreateAsync(
                expectedKey,
                It.IsAny<Func<CancellationToken, ValueTask<HashSet<string>?>>>(),
                It.IsAny<CachingEntryOption?>(),
                It.IsAny<IEnumerable<string>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPerms);

        Result<HashSet<string>?> result = await _sut.GetAsync(userId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedPerms);
    }

    [Fact(DisplayName = "PermissionCache: GetAsync should return null on cache miss")]
    public async Task GetAsync_ShouldReturnNull_WhenCacheMiss()
    {
        Guid userId = Guid.NewGuid();
        string expectedKey = $"perm:user:{userId}";

        _cacheServiceMock
            .Setup(x => x.GetOrCreateAsync(
                expectedKey,
                It.IsAny<Func<CancellationToken, ValueTask<HashSet<string>?>>>(),
                It.IsAny<CachingEntryOption?>(),
                It.IsAny<IEnumerable<string>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((HashSet<string>?)null);

        Result<HashSet<string>?> result = await _sut.GetAsync(userId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact(DisplayName = "PermissionCache: SetUserAsync should store permissions with correct key and tags")]
    public async Task SetUserAsync_ShouldStorePermissions_WithCorrectKeyAndTags()
    {
        Guid userId = Guid.NewGuid();
        var permissions = new HashSet<string> { "perm1" };
        string expectedKey = $"perm:user:{userId}";

        await _sut.SetUserAsync(userId, permissions, ct: TestContext.Current.CancellationToken);

        _cacheServiceMock.Verify(x => x.SetAsync(
            expectedKey,
            permissions,
            It.Is<CachingEntryOption?>(o =>
                o != null &&
                o.Expiration == _authzSetting.PermissionCache.AbsoluteExpiration &&
                o.LocalCacheExpiration == _authzSetting.PermissionCache.SlidingExpiration),
            It.Is<IEnumerable<string>?>(tags =>
                tags != null &&
                tags.Contains($"perm:user:{userId}") &&
                tags.Contains("perm:global")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PermissionCache: SetUserAsync should include role tags when provided")]
    public async Task SetUserAsync_ShouldIncludeRoleTags_WhenRoleIdsProvided()
    {
        Guid userId = Guid.NewGuid();
        Guid roleId1 = Guid.NewGuid();
        Guid roleId2 = Guid.NewGuid();
        var permissions = new HashSet<string> { "perm1" };
        Guid[] roleIds = new[] { roleId1, roleId2 };

        await _sut.SetUserAsync(userId, permissions, roleIds, TestContext.Current.CancellationToken);

        _cacheServiceMock.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            permissions,
            It.IsAny<CachingEntryOption?>(),
            It.Is<IEnumerable<string>?>(tags =>
                tags != null &&
                tags.Contains($"perm:role:{roleId1}") &&
                tags.Contains($"perm:role:{roleId2}")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PermissionCache: InvalidateUserAsync should remove key and tag")]
    public async Task InvalidateUserAsync_ShouldRemoveKeyAndTag()
    {
        Guid userId = Guid.NewGuid();
        string expectedKey = $"perm:user:{userId}";
        string expectedTag = $"perm:user:{userId}";

        await _sut.InvalidateUserAsync(userId, TestContext.Current.CancellationToken);

        _cacheServiceMock.Verify(x => x.RemoveAsync(expectedKey, It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveByTagAsync(expectedTag, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PermissionCache: InvalidateAllAsync should remove by global tag")]
    public async Task InvalidateAllAsync_ShouldRemoveByGlobalTag()
    {
        await _sut.InvalidateAllAsync(TestContext.Current.CancellationToken);

        _cacheServiceMock.Verify(x => x.RemoveByTagAsync("perm:global", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PermissionCache: GetRoleAsync should return cached permissions on hit")]
    public async Task GetRoleAsync_ShouldReturnCachedPermissions_WhenCacheHit()
    {
        Guid roleId = Guid.NewGuid();
        var expectedPerms = new HashSet<string> { "role.perm1" };
        string expectedKey = $"perm:role:{roleId}";

        _cacheServiceMock
            .Setup(x => x.GetOrCreateAsync(
                expectedKey,
                It.IsAny<Func<CancellationToken, ValueTask<HashSet<string>?>>>(),
                It.IsAny<CachingEntryOption?>(),
                It.IsAny<IEnumerable<string>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPerms);

        Result<HashSet<string>?> result = await _sut.GetRoleAsync(roleId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedPerms);
    }

    [Fact(DisplayName = "PermissionCache: GetRoleAsync should return null on cache miss")]
    public async Task GetRoleAsync_ShouldReturnNull_WhenCacheMiss()
    {
        Guid roleId = Guid.NewGuid();
        string expectedKey = $"perm:role:{roleId}";

        _cacheServiceMock
            .Setup(x => x.GetOrCreateAsync(
                expectedKey,
                It.IsAny<Func<CancellationToken, ValueTask<HashSet<string>?>>>(),
                It.IsAny<CachingEntryOption?>(),
                It.IsAny<IEnumerable<string>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((HashSet<string>?)null);

        Result<HashSet<string>?> result = await _sut.GetRoleAsync(roleId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact(DisplayName = "PermissionCache: SetRoleAsync should store permissions with correct key and tags")]
    public async Task SetRoleAsync_ShouldStorePermissions_WithCorrectKeyAndTags()
    {
        Guid roleId = Guid.NewGuid();
        var permissions = new HashSet<string> { "role.perm1" };
        string expectedKey = $"perm:role:{roleId}";

        await _sut.SetRoleAsync(roleId, permissions, TestContext.Current.CancellationToken);

        _cacheServiceMock.Verify(x => x.SetAsync(
            expectedKey,
            permissions,
            It.Is<CachingEntryOption?>(o =>
                o != null &&
                o.Expiration == _authzSetting.PermissionCache.AbsoluteExpiration &&
                o.LocalCacheExpiration == _authzSetting.PermissionCache.SlidingExpiration),
            It.Is<IEnumerable<string>?>(tags =>
                tags != null &&
                tags.Contains($"perm:role:{roleId}") &&
                tags.Contains("perm:global")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PermissionCache: InvalidateRoleAsync should remove role key and tags")]
    public async Task InvalidateRoleAsync_ShouldRemoveRoleKeyAndTags()
    {
        Guid roleId = Guid.NewGuid();
        string expectedKey = $"perm:role:{roleId}";
        string expectedTag = $"perm:role:{roleId}";

        await _sut.InvalidateRoleAsync(roleId, TestContext.Current.CancellationToken);

        _cacheServiceMock.Verify(x => x.RemoveAsync(expectedKey, It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveByTagAsync(expectedTag, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PermissionCache: SetUserAsync should use configured TTL from options")]
    public async Task SetUserAsync_ShouldUseConfiguredTTL()
    {
        Guid userId = Guid.NewGuid();
        var permissions = new HashSet<string> { "perm1" };

        var altSetting = new AuthzSetting
        {
            PermissionCache = new PermissionCacheOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(10),
                AbsoluteExpiration = TimeSpan.FromHours(1),
            }
        };

        _authzOptionsMock.Setup(x => x.Value).Returns(altSetting);

        await _sut.SetUserAsync(userId, permissions, ct: TestContext.Current.CancellationToken);

        _cacheServiceMock.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            permissions,
            It.Is<CachingEntryOption?>(o =>
                o != null &&
                o.Expiration!.Value == TimeSpan.FromHours(1) &&
                o.LocalCacheExpiration!.Value == TimeSpan.FromMinutes(10)),
            It.IsAny<IEnumerable<string>?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
