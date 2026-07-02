using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Admin.Users.Update;
using Module.UnitTests.Identity.Fixtures;

using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Admin.Users.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "UserUpdate")]
public class UpdateUserTests
{
    private Mock<UserManager<User>> _userManagerMock = null!;
    private Mock<ICurrentUser> _currentUserMock = null!;
    private readonly Mock<ILogger<UpdateUser.CommandHandler>> _loggerMock = new();
    private UpdateUser.CommandHandler CreateCommandHandler()
    {
        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        return new UpdateUser.CommandHandler(_userManagerMock.Object, _currentUserMock.Object, _loggerMock.Object);
    }

    [Fact(DisplayName = "UseCase: Should update user when user exists")]
    public async Task Handle_ShouldUpdateUser_WhenUserExists()
    {
        // Arrange
        var handler = CreateCommandHandler();
        var userId = Guid.NewGuid();
        var request = new UpdateUser.Request
        {
            Id = userId,
            Email = "updated@example.com",
            UserName = "updateduser",
            FirstName = "Updated",
            LastName = "User",
            PhoneNumber = "123456789",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true
        };

        var user = new User { Id = userId, Email = "old@example.com", UserName = "olduser" };

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.FindByEmailAsync(request.Email)).ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.FindByNameAsync(request.UserName)).ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await handler.Handle(new UpdateUser.Command(userId, request), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be(request.Email);

        _userManagerMock.Verify(m => m.UpdateAsync(user), Times.Once);
    }

    [Fact(DisplayName = "UseCase: Should set ModifiedAtUtc on successful update")]
    public async Task Handle_ShouldSetModifiedAtUtc_WhenSuccessful()
    {
        // Arrange
        var handler = CreateCommandHandler();
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, ModifiedAtUtc = null };
        var request = new UpdateUser.Request { Id = userId, Email = "new@test.com" };

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.FindByEmailAsync(request.Email)).ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.FindByNameAsync(request.UserName)).ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        await handler.Handle(new UpdateUser.Command(userId, request), TestContext.Current.CancellationToken);

        // Assert
        user.ModifiedAtUtc.Should().NotBeNull();
    }

    [Fact(DisplayName = "UseCase: Should return EmailDuplicate when email exists for another user")]
    public async Task Handle_ShouldReturnEmailDuplicate_WhenEmailExistsForAnotherUser()
    {
        // Arrange
        var handler = CreateCommandHandler();
        var userId = Guid.NewGuid();
        var request = new UpdateUser.Request { Id = userId, Email = "existing@example.com" };

        var user = new User { Id = userId };
        var otherUser = new User { Id = Guid.NewGuid(), Email = "existing@example.com" };

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.FindByEmailAsync(request.Email)).ReturnsAsync(otherUser);

        // Act
        var result = await handler.Handle(new UpdateUser.Command(userId, request), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.EmailDuplicate.Code);
    }

    [Fact(DisplayName = "UseCase: Should return UsernameDuplicate when username exists for another user")]
    public async Task Handle_ShouldReturnUsernameDuplicate_WhenUsernameExistsForAnotherUser()
    {
        // Arrange
        var handler = CreateCommandHandler();
        var userId = Guid.NewGuid();
        var request = new UpdateUser.Request { Id = userId, UserName = "existinguser" };

        var user = new User { Id = userId };
        var otherUser = new User { Id = Guid.NewGuid(), UserName = "existinguser" };

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.FindByNameAsync(request.UserName)).ReturnsAsync(otherUser);

        // Act
        var result = await handler.Handle(new UpdateUser.Command(userId, request), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.UsernameDuplicate.Code);
    }

    [Fact(DisplayName = "UseCase: Should return NotFound when user does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var handler = CreateCommandHandler();
        var userId = Guid.NewGuid();
        var request = new UpdateUser.Request { Id = userId };
        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync((User?)null);

        // Act
        var result = await handler.Handle(new UpdateUser.Command(userId, request), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "UseCase: Should return failure when UpdateAsync fails")]
    public async Task Handle_ShouldReturnFailure_WhenUpdateAsyncFails()
    {
        // Arrange
        var handler = CreateCommandHandler();
        var userId = Guid.NewGuid();
        var request = new UpdateUser.Request { Id = userId };
        var user = new User { Id = userId };

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Fail" }));

        // Act
        var result = await handler.Handle(new UpdateUser.Command(userId, request), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "UseCase: Should succeed when user exists")]
    public async Task Handle_ShouldSucceed_WhenUserExists()
    {
        // Arrange
        var handler = CreateCommandHandler();
        var userId = Guid.NewGuid();
        var request = new UpdateUser.Request { Id = userId, Email = "new@test.com" };
        var user = new User { Id = userId };

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.FindByEmailAsync(request.Email)).ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.FindByNameAsync(request.UserName)).ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await handler.Handle(new UpdateUser.Command(userId, request), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userManagerMock.Verify(m => m.UpdateAsync(user), Times.Once);
    }
}
