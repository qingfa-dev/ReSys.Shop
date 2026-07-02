using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Admin.Roles.Delete;

using Shared.Security.Identity.Domain.Roles;

namespace Module.UnitTests.Identity.Features.Admin.Roles.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "RoleDelete")]
public class DeleteRoleTests
{
    private readonly Mock<RoleManager<Role>> _roleManagerMock;
    private readonly Mock<ILogger<DeleteRole.CommandHandler>> _loggerMock = new();
    private readonly DeleteRole.CommandHandler _handler;

    public DeleteRoleTests()
    {
        var roleStoreMock = new Mock<IRoleStore<Role>>();
        _roleManagerMock = new Mock<RoleManager<Role>>(roleStoreMock.Object, null!, null!, null!, null!);
        _handler = new DeleteRole.CommandHandler(_roleManagerMock.Object, _loggerMock.Object);
    }

    [Fact(DisplayName = "Should return NotFound when role doesn't exist")]
    public async Task Handle_ShouldReturnNotFound_WhenRoleNotFound()
    {
        // Arrange
        var request = new DeleteRole.Request { Id = Guid.NewGuid() };
        _roleManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Role?)null);

        // Act
        var result = await _handler.Handle(
            new DeleteRole.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(RoleResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Should return SystemRoleProtected when system role deleted")]
    public async Task Handle_ShouldReturnSystemRoleProtected_WhenRoleIsSystem()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var request = new DeleteRole.Request { Id = roleId };
        var role = new Role { Id = roleId, IsSystem = true };
        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);

        // Act
        var result = await _handler.Handle(
            new DeleteRole.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(RoleResult.Failure.SystemRoleProtected.Code);
    }

    [Fact(DisplayName = "Should return success when role deleted")]
    public async Task Handle_ShouldReturnSuccess_WhenRoleDeleted()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var request = new DeleteRole.Request { Id = roleId };
        var role = new Role { Id = roleId, Name = "ToBeDeleted", IsSystem = false };
        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);
        _roleManagerMock.Setup(m => m.DeleteAsync(role))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(
            new DeleteRole.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("ToBeDeleted");
    }
}
