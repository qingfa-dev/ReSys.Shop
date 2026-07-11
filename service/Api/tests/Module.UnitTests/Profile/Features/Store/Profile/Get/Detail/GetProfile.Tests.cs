using Module.Profile.Domain;
using Module.Profile.Features.Store.Profiles.Get.Detail;
using Module.UnitTests.Identity.Fixtures;
using Module.UnitTests.Profile.Domain;

using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Profile.Features.Store.Profile.Get.Detail;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "ProfileGet")]
public class GetProfileTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;

    public GetProfileTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _currentUserMock = IdentityMocks.CreateCurrentUserMock(Guid.NewGuid());
    }

    private GetProfile.QueryHandler CreateHandler()
        => new(_currentUserMock.Object, _dbContext);

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    // Guard: Reject unauthenticated requests with 401
    [Fact(DisplayName = "UseCase: Should return Unauthorized when user not authenticated")]
    public async Task ExecuteAsync_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(false);

        var handler = CreateHandler();
        var request = new GetProfile.Query(Guid.NewGuid());

        var result = await handler.Handle(request, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserProfileResult.Failure.AuthRequired.Code);
    }

    // Check: Return UserNotFound when identity user does not exist in store
    [Fact(DisplayName = "UseCase: Should return NotFound when user does not exist")]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(x => x.UserId).Returns(userId.ToString());

        var handler = CreateHandler();
        var request = new GetProfile.Query(userId);

        var result = await handler.Handle(request, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserProfileResult.Failure.UserNotFound.Code);
    }

    // Check: Resolve UserProfile from persistence store
    // Map: Return full ProfileDetailResponse with user auth state
    [Fact(DisplayName = "UseCase: Should return profile when user and profile exist")]
    public async Task ExecuteAsync_ShouldReturnProfile_WhenUserAndProfileExist()
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

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(x => x.UserId).Returns(userId.ToString());

        var handler = CreateHandler();
        var request = new GetProfile.Query(userId);

        var result = await handler.Handle(request, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Doe");
        result.Value.FullName.Should().Be("John Doe");
        result.Value.Email.Should().Be("john@test.com");
        result.Value.PhoneNumber.Should().Be("+1234567890");
        result.Value.EmailConfirmed.Should().BeTrue();
        result.Value.PhoneNumberConfirmed.Should().BeFalse();
    }

    [Fact(DisplayName = "UseCase: Should return NotFound when profile does not exist")]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenProfileDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, UserName = "testuser", Email = "test@example.com", IsActive = true };

        _dbContext.Set<User>().Add(user);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(x => x.UserId).Returns(userId.ToString());

        var handler = CreateHandler();
        var request = new GetProfile.Query(userId);

        var result = await handler.Handle(request, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserProfileResult.Failure.NotFound.Code);
    }
}
