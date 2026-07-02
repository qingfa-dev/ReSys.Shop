using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Admin.Roles.Create;

using Shared.Security.Identity.Domain.Roles;

namespace Module.UnitTests.Identity.Features.Admin.Roles.Create;

/// <summary>
/// Contains unit tests for the <see cref="CreateRole.CommandHandler"/>.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "RoleCreate")]
public class CreateRoleTests
{
    private readonly Mock<RoleManager<Role>> _roleManagerMock;
    private readonly Mock<ILogger<CreateRole.CommandHandler>> _loggerMock = new();
    private readonly CreateRole.CommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateRoleTests"/> class.
    /// </summary>
    public CreateRoleTests()
    {
        // Create: Mock instances for RoleManager dependencies
        var roleStoreMock = new Mock<IRoleStore<Role>>();
        _roleManagerMock = new Mock<RoleManager<Role>>(roleStoreMock.Object, null!, null!, null!, null!);

        // Create: The command handler with mocked dependencies
        _handler = new CreateRole.CommandHandler(_roleManagerMock.Object, _loggerMock.Object);
    }

    [Fact(DisplayName = "Should return NameDuplicate when role name exists")]
    public async Task Handle_ShouldReturnNameDuplicate_WhenRoleNameExists()
    {
        // Arrange: Prepare a request with an existing role name and mock RoleManager behavior
        var request = new CreateRole.Request { Name = "Admin" };
        _roleManagerMock.Setup(m => m.FindByNameAsync("Admin"))
            .ReturnsAsync(new Role { Name = "Admin" });

        // Act: Execute the command handler
        var result = await _handler.Handle(
            new CreateRole.Command(request),
            TestContext.Current.CancellationToken);

        // Assert: Verify that the result indicates a name duplicate failure
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(RoleResult.Failure.AlreadyExists.Code);
    }

    [Fact(DisplayName = "Should return success when role created")]
    public async Task Handle_ShouldReturnSuccess_WhenRoleCreated()
    {
        // Arrange: Prepare a request for a new role and mock RoleManager behavior
        var request = new CreateRole.Request { Name = "User", Description = "Standard user" };
        _roleManagerMock.Setup(m => m.FindByNameAsync("User"))
            .ReturnsAsync((Role?)null); // Simulate role not found
        _roleManagerMock.Setup(m => m.CreateAsync(It.IsAny<Role>()))
            .ReturnsAsync(IdentityResult.Success); // Simulate successful role creation

        // Act: Execute the command handler
        var result = await _handler.Handle(
            new CreateRole.Command(request),
            TestContext.Current.CancellationToken);

        // Assert: Verify that the result indicates success and the role name matches
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("User");
    }
}
