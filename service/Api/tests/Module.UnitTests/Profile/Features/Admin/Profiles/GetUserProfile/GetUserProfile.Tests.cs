using Module.Profile.Domain;
using Module.Profile.Features.Admin.Profiles.GetUserProfile;
using Module.UnitTests.Profile.Domain;

using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Profile.Features.Admin.Profile.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Profile")]
[Trait("Feature", "AdminGetUserProfile")]
public class GetUserProfileTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetUserProfile.QueryHandler _handler;

    public GetUserProfileTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new GetUserProfile.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should return profile when user and profile exist")]
    public async Task Handle_ShouldReturnProfile_WhenUserAndProfileExist()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            UserName = "testuser",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            PhoneNumber = "+1234567890",
            DateOfBirth = new DateTimeOffset(2000, 5, 15, 0, 0, 0, TimeSpan.Zero),
            EmailConfirmed = true,
            PhoneNumberConfirmed = false,
            IsActive = true
        };

        var profile = ProfileUserFactory.Create(userId);
        profile.FirstName = "John";
        profile.LastName = "Doe";
        profile.Email = "john@test.com";
        profile.PhoneNumber = "+1234567890";
        profile.DateOfBirth = new DateTimeOffset(2000, 5, 15, 0, 0, 0, TimeSpan.Zero);

        _dbContext.Set<User>().Add(user);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetUserProfile.Query(userId), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Doe");
        result.Value.FullName.Should().Be("John Doe");
        result.Value.Email.Should().Be("john@test.com");
        result.Value.PhoneNumber.Should().Be("+1234567890");
    }

    [Fact(DisplayName = "Handle: Should return UserNotFound when user does not exist")]
    public async Task Handle_ShouldFail_WhenUserNotFound()
    {
        var result = await _handler.Handle(new GetUserProfile.Query(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserProfileResult.Failure.UserNotFound.Code);
    }

    [Fact(DisplayName = "Handle: Should return NotFound when profile does not exist")]
    public async Task Handle_ShouldFail_WhenProfileNotFound()
    {
        var userId = Guid.NewGuid();
        _dbContext.Set<User>().Add(new User { Id = userId, UserName = "testuser", Email = "test@example.com", IsActive = true });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetUserProfile.Query(userId), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserProfileResult.Failure.NotFound.Code);
    }
}
