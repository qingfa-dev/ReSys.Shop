using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Module.Identity.Features.Store.Auth.Login.Password;

using Shared.Operational.Persistence.Data;
using Shared.Security.Authentication.Tokens.Services.Access;
using Shared.Security.Authentication.Tokens.Services.Refresh;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Store.Auth.Login.Password;

public class PasswordLoginPhoneLookupTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly PasswordLogin.CommandHandler _handler;

    public PasswordLoginPhoneLookupTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new ApplicationDbContext(options);

        var userStore = new UserStore<User, Role, ApplicationDbContext, Guid>(_dbContext);
        var userManager = new UserManager<User>(
            userStore, null!, null!, null!, null!, null!, null!, null!, null!);

        var signInManager = new Mock<SignInManager<User>>(
            userManager,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<User>>(),
            null!, null!, null!, null!).Object;

        _handler = new PasswordLogin.CommandHandler(
            Mock.Of<ISystemDateTime>(),
            signInManager,
            userManager,
            Mock.Of<IAccessTokenService>(),
            Mock.Of<IRefreshTokenService>(),
            Mock.Of<ICurrentUser>(),
            Mock.Of<ILogger<PasswordLogin.CommandHandler>>());
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task FindUserByCredentialAsync_Should_Find_User_By_PhoneNumber()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "testuser",
            PhoneNumber = "+15551234567",
            EmailConfirmed = true
        };
        user.NormalizedEmail = user.Email;
        user.NormalizedUserName = user.UserName;
        await _dbContext.Set<User>().AddAsync(user);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var found = await _handler.FindUserByCredentialAsync("+15551234567");

        found.Should().NotBeNull();
        found!.PhoneNumber.Should().Be("+15551234567");
    }

    [Fact]
    public async Task FindUserByCredentialAsync_Should_Find_User_By_Email_Before_Phone()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "alice@example.com",
            UserName = "alice",
            PhoneNumber = "+15551112222"
        };
        user.NormalizedEmail = user.Email;
        user.NormalizedUserName = user.UserName;
        await _dbContext.Set<User>().AddAsync(user);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var found = await _handler.FindUserByCredentialAsync("alice@example.com");

        found.Should().NotBeNull();
        found!.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public async Task FindUserByCredentialAsync_Should_Find_User_By_UserName()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "bob@example.com",
            UserName = "bob",
            PhoneNumber = "+15551113333"
        };
        user.NormalizedEmail = user.Email;
        user.NormalizedUserName = user.UserName;
        await _dbContext.Set<User>().AddAsync(user);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var found = await _handler.FindUserByCredentialAsync("bob");

        found.Should().NotBeNull();
        found!.UserName.Should().Be("bob");
    }

    [Fact]
    public async Task FindUserByCredentialAsync_Should_Return_Null_When_NotFound()
    {
        var found = await _handler.FindUserByCredentialAsync("+99999999999");

        found.Should().BeNull();
    }
}
