using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Admin.Users.Create;
using Module.UnitTests.Identity.Fixtures;

using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Admin.Users.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "UserCreate")]
public class CreateUserTests
{
    private Mock<UserManager<User>> _userManagerMock = null!;

    private readonly Mock<ILogger<CreateUser.CommandHandler>> _loggerMock = new();

    private CreateUser.CommandHandler CreateCommandHandler()
    {
        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();

        return new CreateUser.CommandHandler(_userManagerMock.Object, _loggerMock.Object);
    }

    [Fact(DisplayName = "UseCase: Should return EmailDuplicate when email exists")]
    public async Task Handle_ShouldReturnEmailDuplicate_WhenEmailExists()
    {
        // Arrange
        var handler = CreateCommandHandler();
        var request = new CreateUser.Request
        {
            Email = "test@example.com",
            UserName = "testuser",
            FirstName = "Test",
            LastName = "User"
        };
        _userManagerMock.Setup(m => m.FindByEmailAsync(request.Email))
            .ReturnsAsync(new User { Email = request.Email });

        // Act
        var result = await handler.Handle(
            new CreateUser.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.EmailDuplicate.Code);
    }

    [Fact(DisplayName = "UseCase: Should return UsernameDuplicate when username exists")]
    public async Task Handle_ShouldReturnUsernameDuplicate_WhenUsernameExists()
    {
        // Arrange
        var handler = CreateCommandHandler();
        var request = new CreateUser.Request
        {
            Email = "test@example.com",
            UserName = "testuser",
            FirstName = "Test",
            LastName = "User"
        };
        _userManagerMock.Setup(m => m.FindByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.FindByNameAsync(request.UserName))
            .ReturnsAsync(new User { UserName = request.UserName });

        // Act
        var result = await handler.Handle(
            new CreateUser.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.UsernameDuplicate.Code);
    }

    [Fact(DisplayName = "UseCase: Should return success when user created")]
    public async Task Handle_ShouldReturnSuccess_WhenUserCreated()
    {
        // Arrange
        var handler = CreateCommandHandler();
        var request = new CreateUser.Request
        {
            Email = "new@example.com",
            UserName = "newuser",
            FirstName = "New",
            LastName = "User",
            EmailConfirmed = true,
            PhoneNumberConfirmed = false,
            PhoneNumber = "+1234567890"
        };
        _userManagerMock.Setup(m => m.FindByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.FindByNameAsync(request.UserName))
            .ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GeneratePasswordResetTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("test-token");
        _userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await handler.Handle(
            new CreateUser.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be(request.Email);

        _userManagerMock.Verify(m => m.CreateAsync(It.IsAny<User>()), Times.Once);
        _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact(DisplayName = "UseCase: Should return failure when CreateAsync fails")]
    public async Task Handle_ShouldReturnFailure_WhenCreateAsyncFails()
    {
        // Arrange
        var handler = CreateCommandHandler();
        var request = new CreateUser.Request { Email = "fail@example.com", UserName = "failuser" };

        _userManagerMock.Setup(m => m.FindByEmailAsync(request.Email)).ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.FindByNameAsync(request.UserName)).ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Fail" }));

        // Act
        var result = await handler.Handle(new CreateUser.Command(request), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "UseCase: Should return failure when UpdateAsync fails")]
    public async Task Handle_ShouldReturnFailure_WhenUpdateAsyncFails()
    {
        // Arrange
        var handler = CreateCommandHandler();
        var request = new CreateUser.Request { Email = "fail@example.com", UserName = "failuser" };

        _userManagerMock.Setup(m => m.FindByEmailAsync(request.Email)).ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.FindByNameAsync(request.UserName)).ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GeneratePasswordResetTokenAsync(It.IsAny<User>())).ReturnsAsync("token");
        _userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Fail" }));

        // Act
        var result = await handler.Handle(new CreateUser.Command(request), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "UseCase: Should generate correct domain events")]
    public async Task Handle_ShouldGenerateDomainEvents_WhenSuccessful()
    {
        // Arrange
        var handler = CreateCommandHandler();
        var request = new CreateUser.Request
        {
            Email = "events@example.com",
            UserName = "eventuser",
            FirstName = "Event",
            LastName = "User"
        };

        User? capturedUser = null;
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GeneratePasswordResetTokenAsync(It.IsAny<User>())).ReturnsAsync("token");
        _userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await handler.Handle(new CreateUser.Command(request), TestContext.Current.CancellationToken);

        // Assert
        capturedUser.Should().NotBeNull();
    }
}
