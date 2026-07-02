using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Shared.Operational.Persistence.Data;
using Shared.Security.Authorization.Permissions.Store;
using Shared.Security.Identity.Domain.Permissions;
using Shared.Security.Identity.Domain.Roles.Claims;
using Shared.Security.Identity.Domain.Users.Claims;
using Shared.Security.Identity.Domain.Users.Roles;

namespace Shared.UnitTests.Security.Authorization.Permissions.Store;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "PermissionStore")]
public sealed class PermissionStoreTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly PermissionStoreService _sut;
    private readonly Mock<ILogger<PermissionStoreService>> _loggerMock = new();

    public PermissionStoreTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _sut = new PermissionStoreService(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "PermissionStore: Should return empty set when user has no permissions")]
    public async Task GetPermissionsAsync_ShouldReturnEmptySet_WhenUserHasNoPermissions()
    {
        Guid userId = Guid.NewGuid();

        Result<HashSet<string>> result = await _sut.GetUserPermissionsAsync(userId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact(DisplayName = "PermissionStore: Should return permissions from role claims")]
    public async Task GetPermissionsAsync_ShouldReturnRoleClaims_WhenUserHasRoles()
    {
        Guid userId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();
        string permission = "store.catalog.product.read";

        _dbContext.Set<UserRole>().Add(new UserRole { UserId = userId, RoleId = roleId });
        _dbContext.Set<RoleClaim>().Add(new RoleClaim { RoleId = roleId, ClaimType = PermissionMetadataConstant.ClaimType, ClaimValue = permission });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<HashSet<string>> result = await _sut.GetUserPermissionsAsync(userId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain(permission);
    }

    [Fact(DisplayName = "PermissionStore: Should return permissions from direct user claims")]
    public async Task GetPermissionsAsync_ShouldReturnUserClaims_WhenUserHasDirectClaims()
    {
        Guid userId = Guid.NewGuid();
        string permission = "store.catalog.product.create";

        _dbContext.Set<UserClaim>().Add(new UserClaim { UserId = userId, ClaimType = PermissionMetadataConstant.ClaimType, ClaimValue = permission });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<HashSet<string>> result = await _sut.GetUserPermissionsAsync(userId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain(permission);
    }

    [Fact(DisplayName = "PermissionStore: Should merge role and user claims into a distinct set")]
    public async Task GetPermissionsAsync_ShouldReturnMergedDistinctSet_WhenUserHasBoth()
    {
        Guid userId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();
        string p1 = "store.catalog.product.read";
        string p2 = "store.catalog.product.delete";

        _dbContext.Set<UserRole>().Add(new UserRole { UserId = userId, RoleId = roleId });
        _dbContext.Set<RoleClaim>().Add(new RoleClaim { RoleId = roleId, ClaimType = PermissionMetadataConstant.ClaimType, ClaimValue = p1 });
        _dbContext.Set<UserClaim>().Add(new UserClaim { UserId = userId, ClaimType = PermissionMetadataConstant.ClaimType, ClaimValue = p2 });
        _dbContext.Set<UserClaim>().Add(new UserClaim { UserId = userId, ClaimType = PermissionMetadataConstant.ClaimType, ClaimValue = p1 });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<HashSet<string>> result = await _sut.GetUserPermissionsAsync(userId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(p1);
        result.Value.Should().Contain(p2);
    }

    [Fact(DisplayName = "PermissionStore: Should return permissions for a specific role")]
    public async Task GetRolePermissionsAsync_ShouldReturnRoleClaims()
    {
        Guid roleId = Guid.NewGuid();
        string permission = "store.catalog.product.read";

        _dbContext.Set<RoleClaim>().Add(new RoleClaim { RoleId = roleId, ClaimType = PermissionMetadataConstant.ClaimType, ClaimValue = permission });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<HashSet<string>> result = await _sut.GetRolePermissionsAsync(roleId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain(permission);
    }

    [Fact(DisplayName = "PermissionStore: Should return role IDs for a user")]
    public async Task GetUserRoleIdsAsync_ShouldReturnRoleGuids()
    {
        Guid userId = Guid.NewGuid();
        Guid roleId1 = Guid.NewGuid();
        Guid roleId2 = Guid.NewGuid();

        _dbContext.Set<UserRole>().Add(new UserRole { UserId = userId, RoleId = roleId1 });
        _dbContext.Set<UserRole>().Add(new UserRole { UserId = userId, RoleId = roleId2 });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<HashSet<Guid>> result = await _sut.GetUserRoleIdsAsync(userId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(roleId1);
        result.Value.Should().Contain(roleId2);
    }

    [Fact(DisplayName = "PermissionStore: Should return ONLY direct user claims")]
    public async Task GetUserDirectPermissionsAsync_ShouldReturnOnlyUserClaims()
    {
        Guid userId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();
        string userPerm = "store.catalog.product.create";
        string rolePerm = "store.catalog.product.read";

        _dbContext.Set<UserRole>().Add(new UserRole { UserId = userId, RoleId = roleId });
        _dbContext.Set<RoleClaim>().Add(new RoleClaim { RoleId = roleId, ClaimType = PermissionMetadataConstant.ClaimType, ClaimValue = rolePerm });
        _dbContext.Set<UserClaim>().Add(new UserClaim { UserId = userId, ClaimType = PermissionMetadataConstant.ClaimType, ClaimValue = userPerm });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<HashSet<string>> result = await _sut.GetUserDirectPermissionsAsync(userId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain(userPerm);
        result.Value.Should().NotContain(rolePerm);
    }

    [Fact(DisplayName = "PermissionStore: Should batch add and remove role permissions")]
    public async Task AddAndRemoveRolePermissions_ShouldWorkCorrectly()
    {
        Guid roleId = Guid.NewGuid();
        string p1 = "store.catalog.product.read";
        string p2 = "store.catalog.product.delete";

        Result addResult = await _sut.AddRolePermissionsAsync(roleId, [p1, p2], TestContext.Current.CancellationToken);

        addResult.IsSuccess.Should().BeTrue();
        List<RoleClaim> stored = await _dbContext.Set<RoleClaim>().Where(rc => rc.RoleId == roleId).ToListAsync(TestContext.Current.CancellationToken);
        stored.Should().HaveCount(2);

        Result removeResult = await _sut.RemoveRolePermissionsAsync(roleId, [p1], TestContext.Current.CancellationToken);

        removeResult.IsSuccess.Should().BeTrue();
        stored = await _dbContext.Set<RoleClaim>().Where(rc => rc.RoleId == roleId).ToListAsync(TestContext.Current.CancellationToken);
        stored.Should().HaveCount(1);
        stored.First().ClaimValue.Should().Be(p2);
    }

    [Fact(DisplayName = "PermissionStore: Should batch add and remove direct user permissions")]
    public async Task AddAndRemoveUserPermissions_ShouldWorkCorrectly()
    {
        Guid userId = Guid.NewGuid();
        string p1 = "store.catalog.product.create";
        string p2 = "store.catalog.product.delete";

        Result addResult = await _sut.AddUserDirectPermissionsAsync(userId, [p1, p2], TestContext.Current.CancellationToken);

        addResult.IsSuccess.Should().BeTrue();
        List<UserClaim> stored = await _dbContext.Set<UserClaim>().Where(uc => uc.UserId == userId).ToListAsync(TestContext.Current.CancellationToken);
        stored.Should().HaveCount(2);

        Result removeResult = await _sut.RemoveUserDirectPermissionsAsync(userId, [p1], TestContext.Current.CancellationToken);

        removeResult.IsSuccess.Should().BeTrue();
        stored = await _dbContext.Set<UserClaim>().Where(uc => uc.UserId == userId).ToListAsync(TestContext.Current.CancellationToken);
        stored.Should().HaveCount(1);
        stored.First().ClaimValue.Should().Be(p2);
    }

    [Fact(DisplayName = "PermissionStore: Should add duplicate role permissions (no dedup in store layer)")]
    public async Task AddRolePermissionsAsync_DuplicatePermissions_AddsBoth()
    {
        Guid roleId = Guid.NewGuid();
        string p1 = "store.catalog.product.read";

        await _sut.AddRolePermissionsAsync(roleId, [p1], TestContext.Current.CancellationToken);
        await _sut.AddRolePermissionsAsync(roleId, [p1], TestContext.Current.CancellationToken);

        List<RoleClaim> stored = await _dbContext.Set<RoleClaim>().Where(rc => rc.RoleId == roleId).ToListAsync(TestContext.Current.CancellationToken);
        stored.Should().HaveCount(2);
    }

    [Fact(DisplayName = "PermissionStore: Should partially remove role permissions leaving others")]
    public async Task RemoveRolePermissionsAsync_PartialRemoval_LeavesOtherPermissions()
    {
        Guid roleId = Guid.NewGuid();
        string p1 = "store.catalog.product.read";
        string p2 = "store.catalog.product.delete";

        await _sut.AddRolePermissionsAsync(roleId, [p1, p2], TestContext.Current.CancellationToken);

        Result result = await _sut.RemoveRolePermissionsAsync(roleId, [p2], TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        List<RoleClaim> stored = await _dbContext.Set<RoleClaim>().Where(rc => rc.RoleId == roleId).ToListAsync(TestContext.Current.CancellationToken);
        stored.Should().HaveCount(1);
        stored.First().ClaimValue.Should().Be(p1);
    }

    [Fact(DisplayName = "PermissionStore: Should remove all role permissions when all specified")]
    public async Task RemoveRolePermissionsAsync_AllRemoved_ReturnsEmpty()
    {
        Guid roleId = Guid.NewGuid();
        string p1 = "store.catalog.product.read";
        string p2 = "store.catalog.product.delete";

        await _sut.AddRolePermissionsAsync(roleId, [p1, p2], TestContext.Current.CancellationToken);

        Result result = await _sut.RemoveRolePermissionsAsync(roleId, [p1, p2], TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        List<RoleClaim> stored = await _dbContext.Set<RoleClaim>().Where(rc => rc.RoleId == roleId).ToListAsync(TestContext.Current.CancellationToken);
        stored.Should().BeEmpty();
    }

    [Fact(DisplayName = "PermissionStore: Should return empty set when user has no direct claims")]
    public async Task GetUserDirectPermissionsAsync_NoDirectClaims_ReturnsEmpty()
    {
        Guid userId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();

        _dbContext.Set<UserRole>().Add(new UserRole { UserId = userId, RoleId = roleId });
        _dbContext.Set<RoleClaim>().Add(new RoleClaim { RoleId = roleId, ClaimType = PermissionMetadataConstant.ClaimType, ClaimValue = "store.catalog.product.read" });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<HashSet<string>> result = await _sut.GetUserDirectPermissionsAsync(userId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact(DisplayName = "PermissionStore: Should return empty set when user has no roles")]
    public async Task GetUserRoleIdsAsync_NoRoles_ReturnsEmpty()
    {
        Guid userId = Guid.NewGuid();

        Result<HashSet<Guid>> result = await _sut.GetUserRoleIdsAsync(userId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact(DisplayName = "PermissionStore: Should not fail when removing nonexistent role permissions")]
    public async Task RemoveRolePermissionsAsync_NonExistentPermissions_ReturnsSuccess()
    {
        Guid roleId = Guid.NewGuid();

        Result result = await _sut.RemoveRolePermissionsAsync(roleId, ["nonexistent.perm"], TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "PermissionStore: Should handle empty permissions list gracefully")]
    public async Task AddRolePermissionsAsync_EmptyPermissions_ReturnsSuccess()
    {
        Result result = await _sut.AddRolePermissionsAsync(Guid.NewGuid(), [], TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "PermissionStore: Should return empty set for empty database")]
    public async Task GetAllPermissionIdentifiersAsync_ShouldReturnEmptySet_WhenDatabaseIsEmpty()
    {
        Result<HashSet<string>> result = await _sut.GetAllPermissionIdentifiersAsync(TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact(DisplayName = "PermissionStore: Should return distinct identifiers from role claims")]
    public async Task GetAllPermissionIdentifiersAsync_ShouldReturnDistinctIdentifiers_FromRoleClaims()
    {
        _dbContext.Set<RoleClaim>().Add(new RoleClaim { RoleId = Guid.NewGuid(), ClaimType = PermissionMetadataConstant.ClaimType, ClaimValue = "admin.catalog.products.read" });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<HashSet<string>> result = await _sut.GetAllPermissionIdentifiersAsync(TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("admin.catalog.products.read");
        result.Value.Should().HaveCount(1);
    }

    [Fact(DisplayName = "PermissionStore: Should return distinct identifiers from user claims")]
    public async Task GetAllPermissionIdentifiersAsync_ShouldReturnDistinctIdentifiers_FromUserClaims()
    {
        _dbContext.Set<UserClaim>().Add(new UserClaim { UserId = Guid.NewGuid(), ClaimType = PermissionMetadataConstant.ClaimType, ClaimValue = "admin.orders.orders.view" });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<HashSet<string>> result = await _sut.GetAllPermissionIdentifiersAsync(TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("admin.orders.orders.view");
        result.Value.Should().HaveCount(1);
    }

    [Fact(DisplayName = "PermissionStore: Should deduplicate identifiers across role and user claims")]
    public async Task GetAllPermissionIdentifiersAsync_ShouldDeduplicate_AcrossRoleAndUserClaims()
    {
        string perm = "admin.catalog.products.read";
        _dbContext.Set<RoleClaim>().Add(new RoleClaim { RoleId = Guid.NewGuid(), ClaimType = PermissionMetadataConstant.ClaimType, ClaimValue = perm });
        _dbContext.Set<UserClaim>().Add(new UserClaim { UserId = Guid.NewGuid(), ClaimType = PermissionMetadataConstant.ClaimType, ClaimValue = perm });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<HashSet<string>> result = await _sut.GetAllPermissionIdentifiersAsync(TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.Should().Contain(perm);
    }

    [Fact(DisplayName = "PermissionStore: Should filter by claim type and exclude non-permission claims")]
    public async Task GetAllPermissionIdentifiersAsync_ShouldFilterByClaimType_ExcludeNonPermission()
    {
        _dbContext.Set<RoleClaim>().Add(new RoleClaim { RoleId = Guid.NewGuid(), ClaimType = "other", ClaimValue = "some.value" });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<HashSet<string>> result = await _sut.GetAllPermissionIdentifiersAsync(TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}