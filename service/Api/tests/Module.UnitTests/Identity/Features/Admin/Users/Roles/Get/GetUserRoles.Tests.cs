using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Shared.Admin.Users.Roles.Get;
using Module.UnitTests.Identity.Fixtures;

using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Admin.Users.Roles.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "UserRoles")]
public class GetUserRolesTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<RoleManager<Role>> _roleManagerMock;
    private readonly GetUserRoles.PagedQueryHandler _handler;

    public GetUserRolesTests()
    {
        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();
        _roleManagerMock = IdentityMocks.CreateRoleManagerMock<Role>();
        _handler = new GetUserRoles.PagedQueryHandler(_userManagerMock.Object, _roleManagerMock.Object);
    }

    [Fact(DisplayName = "Handler: Should return NotFound when user is not found")]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var query = new GetUserRoles.Query(Guid.NewGuid(), new GetUserRoles.Parameters());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors[0].Code.Should().Be(UserResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should return all roles with assignment status")]
    public async Task Handle_ShouldReturnRolesWithAssignmentStatus()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), UserName = "testuser" };
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        var allRoles = new List<Role>
        {
            new() { Name = "Admin", Description = "Admin Role" },
            new() { Name = "User", Description = "User Role" }
        }.AsQueryable();

        // Note: Mocking IQueryable on RoleManager.Roles is tricky, 
        // normally we use MockQueryable or just mock the property if possible.
        // IdentityMocks.CreateRoleManagerMock might need adjustment if it doesn't support .Roles
        _roleManagerMock.Setup(x => x.Roles).Returns(allRoles);

        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(["Admin"]);

        var query = new GetUserRoles.Query(user.Id, new GetUserRoles.Parameters());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
        result.Items.Should().ContainSingle(r => r.Name == "Admin" && r.IsAssigned);
        result.Items.Should().ContainSingle(r => r.Name == "User" && !r.IsAssigned);
    }
}
