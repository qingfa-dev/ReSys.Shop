using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Shared.Admin.Roles.Get.ById;

using Shared.Security.Identity.Domain.Roles;

namespace Module.UnitTests.Identity.Features.Admin.Roles.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "RoleGetById")]
public class GetRoleByIdTests
{
    private readonly Mock<RoleManager<Role>> _roleManagerMock;
    private readonly GetRoleById.QueryHandler _handler;

    public GetRoleByIdTests()
    {
        var roleStoreMock = new Mock<IRoleStore<Role>>();
        _roleManagerMock = new Mock<RoleManager<Role>>(roleStoreMock.Object, null!, null!, null!, null!);
        _handler = new GetRoleById.QueryHandler(_roleManagerMock.Object);
    }

    [Fact(DisplayName = "Should return NotFound when role doesn't exist")]
    public async Task Handle_ShouldReturnNotFound_WhenRoleNotFound()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        _roleManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Role?)null);

        // Act
        var result = await _handler.Handle(
            new GetRoleById.Query(roleId),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(RoleResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Should return role detail when found")]
    public async Task Handle_ShouldReturnRole_WhenFound()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new Role { Id = roleId, Name = "Admin", Description = "Admin Role" };
        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);

        // Act
        var result = await _handler.Handle(
            new GetRoleById.Query(roleId),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(roleId);
        result.Value.Name.Should().Be("Admin");
    }
}
