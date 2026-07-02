using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Admin.Roles.Update;

using Shared.Security.Identity.Domain.Roles;

namespace Module.UnitTests.Identity.Features.Admin.Roles.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "RoleUpdate")]
public class UpdateRoleTests
{
    private readonly Mock<RoleManager<Role>> _roleManagerMock;
    private readonly Mock<ILogger<UpdateRole.CommandHandler>> _loggerMock = new();
    private readonly UpdateRole.CommandHandler _handler;

    public UpdateRoleTests()
    {
        var roleStoreMock = new Mock<IRoleStore<Role>>();
        _roleManagerMock = new Mock<RoleManager<Role>>(roleStoreMock.Object, null!, null!, null!, null!);
        _handler = new UpdateRole.CommandHandler(_roleManagerMock.Object, _loggerMock.Object);
    }

    [Fact(DisplayName = "Should return NotFound when role doesn't exist")]
    public async Task Handle_ShouldReturnNotFound_WhenRoleNotFound()
    {
        // Arrange
        var request = new UpdateRole.Request { Name = "NewName" };
        _roleManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Role?)null);

        // Act
        var result = await _handler.Handle(
            new UpdateRole.Command(Guid.NewGuid(), request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(RoleResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Should return SystemRoleProtected when system role updated")]
    public async Task Handle_ShouldReturnSystemRoleProtected_WhenRoleIsSystem()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var request = new UpdateRole.Request { Name = "NewName" };
        var role = new Role { Id = roleId, IsSystem = true };
        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);

        // Act
        var result = await _handler.Handle(
            new UpdateRole.Command(roleId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(RoleResult.Failure.SystemRoleProtected.Code);
    }

    [Fact(DisplayName = "Should return success when role updated")]
    public async Task Handle_ShouldReturnSuccess_WhenRoleUpdated()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var request = new UpdateRole.Request { Name = "UpdatedName" };
        var role = new Role { Id = roleId, Name = "OldName", IsSystem = false };
        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);
        _roleManagerMock.Setup(m => m.FindByNameAsync("UpdatedName"))
            .ReturnsAsync((Role?)null);
        _roleManagerMock.Setup(m => m.UpdateAsync(It.IsAny<Role>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(
            new UpdateRole.Command(roleId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("UpdatedName");
    }
}