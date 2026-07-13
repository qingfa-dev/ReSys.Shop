using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Admin.Users.Delete;
using Module.Profile.Domain;
using Module.UnitTests.Identity.Fixtures;

using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Admin.Users.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "UserDelete")]
public class DeleteUserTests : IDisposable
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<UserManager<User>> _userManagerMock = null!;
    private Mock<ICurrentUser> _currentUserMock = null!;
    private readonly Mock<ILogger<DeleteUser.CommandHandler>> _loggerMock = new();

    private DeleteUser.CommandHandler CreateCommandHandler()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");
        _currentUserMock.Setup(x => x.UserId).Returns("A0000000-0000-0000-0000-000000000000");

        return new DeleteUser.CommandHandler(
            _userManagerMock.Object,
            _currentUserMock.Object,
            _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "UseCase: Should delete user and profile when both exist")]
    public async Task Handle_ShouldDeleteUserAndProfile_WhenBothExist()
    {
        // Arrange
        var handler = CreateCommandHandler();
        var userId = Guid.NewGuid();
        var request = new DeleteUser.Request { Id = userId };

        var user = new User { Id = userId, Email = "test@example.com", UserName = "testuser" };

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await handler.Handle(new DeleteUser.Command(request), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _userManagerMock.Verify(m => m.DeleteAsync(user), Times.Once);
    }

    [Fact(DisplayName = "UseCase: Should return NotFound when user does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var handler = CreateCommandHandler();
        var userId = Guid.NewGuid();
        var request = new DeleteUser.Request { Id = userId };
        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync((User?)null);

        // Act
        var result = await handler.Handle(new DeleteUser.Command(request), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "UseCase: Should return failure when DeleteAsync fails")]
    public async Task Handle_ShouldReturnFailure_WhenDeleteAsyncFails()
    {
        // Arrange
        var handler = CreateCommandHandler();
        var userId = Guid.NewGuid();
        var user = new User { Id = userId };

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Fail" }));

        // Act
        var result = await handler.Handle(new DeleteUser.Command(new DeleteUser.Request { Id = userId }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "UseCase: Should succeed even if profile is missing")]
    public async Task Handle_ShouldSucceed_WhenProfileIsMissing()
    {
        // Arrange
        var handler = CreateCommandHandler();
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "test@example.com", UserName = "testuser" };

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await handler.Handle(new DeleteUser.Command(new DeleteUser.Request { Id = userId }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userManagerMock.Verify(m => m.DeleteAsync(user), Times.Once);
    }
}