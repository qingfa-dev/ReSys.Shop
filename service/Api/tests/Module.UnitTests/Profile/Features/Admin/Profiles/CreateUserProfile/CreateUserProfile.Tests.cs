using Module.Profile.Domain;
using Module.Profile.Features.Admin.Profiles.CreateUserProfile;
using Module.UnitTests.Profile.Domain;

using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Profile.Features.Admin.Profile.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Profile")]
[Trait("Feature", "AdminCreateUserProfile")]
public class CreateUserProfileTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CreateUserProfile.CommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public CreateUserProfileTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);

        _dbContext.Set<User>().Add(new User { Id = _userId, UserName = "testuser", Email = "test@example.com" });
        _dbContext.SaveChanges();

        _handler = new CreateUserProfile.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private static CreateUserProfile.Request CreateValidRequest(
        Guid userId,
        string firstName = "John",
        string lastName = "Doe",
        string email = "john.doe@example.com",
        string phoneNumber = "1234567890",
        DateTimeOffset? dateOfBirth = null)
    {
        return new CreateUserProfile.Request
        {
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            DateOfBirth = dateOfBirth
        };
    }

    [Fact(DisplayName = "Handle: Should create a new profile successfully")]
    public async Task Handle_ShouldCreateProfile_WhenUserExists()
    {
        var request = CreateValidRequest(_userId);

        var result = await _handler.Handle(new CreateUserProfile.Command(request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be(request.FirstName);
        result.Value.LastName.Should().Be(request.LastName);
        result.Value.Email.Should().Be(request.Email);
        result.Value.PhoneNumber.Should().Be(request.PhoneNumber);
        result.Value.DateOfBirth.Should().Be(request.DateOfBirth);

        var profile = await _dbContext.Set<UserProfile>().FirstOrDefaultAsync(p => p.UserId == _userId, TestContext.Current.CancellationToken);
        profile.Should().NotBeNull();
        profile!.FirstName.Should().Be(request.FirstName);
        profile.LastName.Should().Be(request.LastName);
        profile.Email.Should().Be(request.Email);
    }

    [Fact(DisplayName = "Handle: Should create profile when DateOfBirth is null")]
    public async Task Handle_ShouldCreateProfile_WhenDateOfBirthIsNull()
    {
        var request = CreateValidRequest(_userId, dateOfBirth: null);

        var result = await _handler.Handle(new CreateUserProfile.Command(request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.DateOfBirth.Should().BeNull();

        var profile = await _dbContext.Set<UserProfile>().FirstOrDefaultAsync(p => p.UserId == _userId, TestContext.Current.CancellationToken);
        profile!.DateOfBirth.Should().BeNull();
    }

    [Fact(DisplayName = "Handle: Should return UserNotFound when user does not exist")]
    public async Task Handle_ShouldFail_WhenUserNotFound()
    {
        var request = CreateValidRequest(Guid.NewGuid());

        var result = await _handler.Handle(new CreateUserProfile.Command(request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserProfileResult.Failure.UserNotFound.Code);
    }

    [Fact(DisplayName = "Handle: Should return ProfileAlreadyExists when profile already exists")]
    public async Task Handle_ShouldFail_WhenProfileAlreadyExists()
    {
        var existingProfile = ProfileUserFactory.Create(_userId);
        _dbContext.Set<UserProfile>().Add(existingProfile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateValidRequest(_userId);

        var result = await _handler.Handle(new CreateUserProfile.Command(request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserProfileResult.Failure.AlreadyExists.Code);
    }
}
