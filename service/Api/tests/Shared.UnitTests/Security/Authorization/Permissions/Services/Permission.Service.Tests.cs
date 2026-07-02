using Microsoft.Extensions.Logging;

using Shared.Security.Authorization.Permissions.Caches;
using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Authorization.Permissions.Store;

namespace Shared.UnitTests.Security.Authorization.Permissions.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "PermissionService")]
public sealed class PermissionServiceTests
{
    private readonly Mock<IPermissionCache> _cacheMock;
    private readonly Mock<IPermissionStore> _storeMock;
    private readonly Mock<ILogger<PermissionService>> _loggerMock;
    private readonly PermissionService _sut;

    public PermissionServiceTests()
    {
        _cacheMock = new Mock<IPermissionCache>();
        _storeMock = new Mock<IPermissionStore>();
        _loggerMock = new Mock<ILogger<PermissionService>>();

        _sut = new PermissionService(
            _cacheMock.Object,
            _storeMock.Object,
            _loggerMock.Object);
    }

    [Fact(DisplayName = "PermissionService: GetEffectiveUserPermissions should return cached value on cache hit")]
    public async Task GetEffectiveUserPermissionsAsync_ShouldReturnCached_WhenCacheHit()
    {
        Guid userId = Guid.NewGuid();
        var cachedPerms = new HashSet<string> { "perm1", "perm2" };

        _cacheMock
            .Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<string>?>.Ok(cachedPerms));

        Result<HashSet<string>> result = await _sut.GetEffectiveUserPermissionsAsync(userId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(cachedPerms);
        _storeMock.Verify(x => x.GetUserRoleIdsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PermissionService: GetEffectiveUserPermissions should resolve from store on cache miss")]
    public async Task GetEffectiveUserPermissionsAsync_ShouldResolveFromStore_WhenCacheMiss()
    {
        Guid userId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();

        _cacheMock
            .Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<string>?>.Ok(null));

        _storeMock
            .Setup(x => x.GetUserRoleIdsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<Guid>>.Ok(new HashSet<Guid> { roleId }));

        _storeMock
            .Setup(x => x.GetUserDirectPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<string>>.Ok(new HashSet<string>()));

        _cacheMock
            .Setup(x => x.GetRoleAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<string>?>.Ok(null));

        var rolePerms = new HashSet<string> { "role.perm1" };
        _storeMock
            .Setup(x => x.GetRolePermissionsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<string>>.Ok(rolePerms));

        _cacheMock
            .Setup(x => x.SetRoleAsync(roleId, rolePerms, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _cacheMock
            .Setup(x => x.SetUserAsync(userId, It.IsAny<HashSet<string>>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        Result<HashSet<string>> result = await _sut.GetEffectiveUserPermissionsAsync(userId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("role.perm1");
    }

    [Fact(DisplayName = "PermissionService: GetEffectiveUserPermissions should merge role and direct permissions")]
    public async Task GetEffectiveUserPermissionsAsync_ShouldMergeRoleAndDirectPermissions()
    {
        Guid userId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();
        var rolePerms = new HashSet<string> { "role.perm1", "shared.perm" };
        var directPerms = new HashSet<string> { "direct.perm1", "shared.perm" };

        _cacheMock
            .Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<string>?>.Ok(null));

        _storeMock
            .Setup(x => x.GetUserRoleIdsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<Guid>>.Ok(new HashSet<Guid> { roleId }));

        _storeMock
            .Setup(x => x.GetUserDirectPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<string>>.Ok(directPerms));

        _cacheMock
            .Setup(x => x.GetRoleAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<string>?>.Ok(null));

        _storeMock
            .Setup(x => x.GetRolePermissionsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<string>>.Ok(rolePerms));

        _cacheMock
            .Setup(x => x.SetRoleAsync(roleId, rolePerms, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _cacheMock
            .Setup(x => x.SetUserAsync(userId, It.IsAny<HashSet<string>>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        Result<HashSet<string>> result = await _sut.GetEffectiveUserPermissionsAsync(userId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("role.perm1");
        result.Value.Should().Contain("direct.perm1");
        result.Value.Should().Contain("shared.perm");
        result.Value.Should().HaveCount(3);
    }

    [Fact(DisplayName = "PermissionService: GetEffectiveUserPermissions should populate user cache after resolution")]
    public async Task GetEffectiveUserPermissionsAsync_ShouldPopulateUserCache()
    {
        Guid userId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();
        var rolePerms = new HashSet<string> { "role.perm1" };

        _cacheMock
            .Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<string>?>.Ok(null));

        _storeMock
            .Setup(x => x.GetUserRoleIdsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<Guid>>.Ok(new HashSet<Guid> { roleId }));

        _storeMock
            .Setup(x => x.GetUserDirectPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<string>>.Ok(new HashSet<string>()));

        _cacheMock
            .Setup(x => x.GetRoleAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<string>?>.Ok(null));

        _storeMock
            .Setup(x => x.GetRolePermissionsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<string>>.Ok(rolePerms));

        _cacheMock
            .Setup(x => x.SetRoleAsync(roleId, rolePerms, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _cacheMock
            .Setup(x => x.SetUserAsync(userId, It.IsAny<HashSet<string>>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        await _sut.GetEffectiveUserPermissionsAsync(userId, TestContext.Current.CancellationToken);

        _cacheMock.Verify(x => x.SetUserAsync(
            userId,
            It.Is<HashSet<string>>(s => s.Contains("role.perm1")),
            It.Is<IEnumerable<Guid>?>(ids => ids != null && ids.Contains(roleId)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PermissionService: GetRolePermissions should return cached value on cache hit")]
    public async Task GetRolePermissionsAsync_ShouldReturnCached_WhenCacheHit()
    {
        Guid roleId = Guid.NewGuid();
        var cachedPerms = new HashSet<string> { "role.perm1" };

        _cacheMock
            .Setup(x => x.GetRoleAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<string>?>.Ok(cachedPerms));

        Result<HashSet<string>> result = await _sut.GetRolePermissionsAsync(roleId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(cachedPerms);
        _storeMock.Verify(x => x.GetRolePermissionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PermissionService: GetRolePermissions should load and cache on cache miss")]
    public async Task GetRolePermissionsAsync_ShouldLoadAndCache_WhenCacheMiss()
    {
        Guid roleId = Guid.NewGuid();
        var storePerms = new HashSet<string> { "role.perm1" };

        _cacheMock
            .Setup(x => x.GetRoleAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<string>?>.Ok(null));

        _storeMock
            .Setup(x => x.GetRolePermissionsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<string>>.Ok(storePerms));

        _cacheMock
            .Setup(x => x.SetRoleAsync(roleId, storePerms, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        Result<HashSet<string>> result = await _sut.GetRolePermissionsAsync(roleId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(storePerms);
        _cacheMock.Verify(x => x.SetRoleAsync(roleId, storePerms, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PermissionService: HasAllPermissions should return true when all match")]
    public async Task HasAllPermissionsAsync_ShouldReturnTrue_WhenAllMatch()
    {
        Guid userId = Guid.NewGuid();
        var userPerms = new HashSet<string> { "perm1", "perm2", "perm3" };

        _cacheMock
            .Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<string>?>.Ok(userPerms));

        Result<bool> result = await _sut.HasAllPermissionsAsync(userId, ["perm1", "perm2"], TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact(DisplayName = "PermissionService: HasAllPermissions should return false when missing")]
    public async Task HasAllPermissionsAsync_ShouldReturnFalse_WhenMissing()
    {
        Guid userId = Guid.NewGuid();
        var userPerms = new HashSet<string> { "perm1" };

        _cacheMock
            .Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<string>?>.Ok(userPerms));

        Result<bool> result = await _sut.HasAllPermissionsAsync(userId, ["perm1", "missing.perm"], TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact(DisplayName = "PermissionService: RoleHasAllPermissions should return true when all match")]
    public async Task RoleHasAllPermissionsAsync_ShouldReturnTrue_WhenAllMatch()
    {
        Guid roleId = Guid.NewGuid();
        var rolePerms = new HashSet<string> { "role.perm1", "role.perm2" };

        _cacheMock
            .Setup(x => x.GetRoleAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<string>?>.Ok(rolePerms));

        Result<bool> result = await _sut.RoleHasAllPermissionsAsync(roleId, ["role.perm1"], TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact(DisplayName = "PermissionService: AddRolePermissions should delegate to store and invalidate cache")]
    public async Task AddRolePermissionsAsync_ShouldDelegateToStoreAndInvalidateCache()
    {
        Guid roleId = Guid.NewGuid();
        string[] permissions = ["new.perm1"];

        _storeMock
            .Setup(x => x.AddRolePermissionsAsync(roleId, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _cacheMock
            .Setup(x => x.InvalidateRoleAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        Result result = await _sut.AddRolePermissionsAsync(roleId, permissions, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        _storeMock.Verify(x => x.AddRolePermissionsAsync(roleId, permissions, It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(x => x.InvalidateRoleAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PermissionService: RemoveRolePermissions should delegate to store and invalidate cache")]
    public async Task RemoveRolePermissionsAsync_ShouldDelegateToStoreAndInvalidateCache()
    {
        Guid roleId = Guid.NewGuid();
        string[] permissions = ["old.perm1"];

        _storeMock
            .Setup(x => x.RemoveRolePermissionsAsync(roleId, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _cacheMock
            .Setup(x => x.InvalidateRoleAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        Result result = await _sut.RemoveRolePermissionsAsync(roleId, permissions, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        _storeMock.Verify(x => x.RemoveRolePermissionsAsync(roleId, permissions, It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(x => x.InvalidateRoleAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PermissionService: AddUserDirectPermissions should delegate to store and invalidate cache")]
    public async Task AddUserDirectPermissionsAsync_ShouldDelegateToStoreAndInvalidateCache()
    {
        Guid userId = Guid.NewGuid();
        string[] permissions = ["user.perm1"];

        _storeMock
            .Setup(x => x.AddUserDirectPermissionsAsync(userId, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _cacheMock
            .Setup(x => x.InvalidateUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        Result result = await _sut.AddUserDirectPermissionsAsync(userId, permissions, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        _storeMock.Verify(x => x.AddUserDirectPermissionsAsync(userId, permissions, It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(x => x.InvalidateUserAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PermissionService: RemoveUserDirectPermissions should delegate to store and invalidate cache")]
    public async Task RemoveUserDirectPermissionsAsync_ShouldDelegateToStoreAndInvalidateCache()
    {
        Guid userId = Guid.NewGuid();
        string[] permissions = ["user.perm1"];

        _storeMock
            .Setup(x => x.RemoveUserDirectPermissionsAsync(userId, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _cacheMock
            .Setup(x => x.InvalidateUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        Result result = await _sut.RemoveUserDirectPermissionsAsync(userId, permissions, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        _storeMock.Verify(x => x.RemoveUserDirectPermissionsAsync(userId, permissions, It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(x => x.InvalidateUserAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PermissionService: InvalidateRolePermissions should delegate to cache")]
    public async Task InvalidateRolePermissionsAsync_ShouldDelegateToCache()
    {
        Guid roleId = Guid.NewGuid();

        _cacheMock
            .Setup(x => x.InvalidateRoleAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        Result result = await _sut.InvalidateRolePermissionsAsync(roleId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        _cacheMock.Verify(x => x.InvalidateRoleAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PermissionService: InvalidateUserPermissions should delegate to cache")]
    public async Task InvalidateUserPermissionsAsync_ShouldDelegateToCache()
    {
        Guid userId = Guid.NewGuid();

        _cacheMock
            .Setup(x => x.InvalidateUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        Result result = await _sut.InvalidateUserPermissionsAsync(userId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        _cacheMock.Verify(x => x.InvalidateUserAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PermissionService: GetEffectiveUserPermissions should return empty set on exception")]
    public async Task GetEffectiveUserPermissionsAsync_ShouldReturnEmpty_OnException()
    {
        Guid userId = Guid.NewGuid();

        _cacheMock
            .Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cache failure"));

        Result<HashSet<string>> result = await _sut.GetEffectiveUserPermissionsAsync(userId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact(DisplayName = "PermissionService: GetRolePermissions should return empty set on exception")]
    public async Task GetRolePermissionsAsync_ShouldReturnEmpty_OnException()
    {
        Guid roleId = Guid.NewGuid();

        _cacheMock
            .Setup(x => x.GetRoleAsync(roleId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cache failure"));

        Result<HashSet<string>> result = await _sut.GetRolePermissionsAsync(roleId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact(DisplayName = "PermissionService: AddRolePermissions should not invalidate cache on store failure")]
    public async Task AddRolePermissionsAsync_ShouldNotInvalidateCache_WhenStoreFails()
    {
        Guid roleId = Guid.NewGuid();
        string[] permissions = ["perm1"];

        _storeMock
            .Setup(x => x.AddRolePermissionsAsync(roleId, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Unexpected(errors: [Error.Unexpected("STORE_FAIL", "Store failed")]));

        Result result = await _sut.AddRolePermissionsAsync(roleId, permissions, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        _cacheMock.Verify(x => x.InvalidateRoleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
