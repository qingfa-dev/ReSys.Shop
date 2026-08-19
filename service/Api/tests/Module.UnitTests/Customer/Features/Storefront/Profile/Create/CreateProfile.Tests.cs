using Module.Customer.Domain;
using Module.Customer.Features.Storefront.Profiles.Create;
using Module.UnitTests.Profile.Domain;

using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Profile.Features.Store.Profile.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "ProfileCreate")]
public class CreateProfileTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CreateProfile.CommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public CreateProfileTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);

        _dbContext.Set<User>().Add(new User { Id = _userId, UserName = "testuser", Email = "test@example.com" });
        _dbContext.SaveChanges();

        _handler = new CreateProfile.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private static CreateProfile.Request CreateValidRequest(
        string firstName = "John",
        string lastName = "Doe",
        string email = "john.doe@example.com",
        string phoneNumber = "1234567890",
        DateTimeOffset? dateOfBirth = null)
    {
        return new CreateProfile.Request
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            DateOfBirth = dateOfBirth
        };
    }

    // Guard: Reject creation when target user does not exist in identity store
    // Create: Build UserProfile via domain builder with full request data
    // Persist: Save new profile and verify all fields round-trip correctly
    [Fact(DisplayName = "Handle: Should create a new profile successfully")]
    public async Task Handle_ShouldCreateProfileSuccessfully()
    {
        var request = CreateValidRequest();

        var result = await _handler.Handle(new CreateProfile.Command(_userId, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be(request.FirstName);
        result.Value.LastName.Should().Be(request.LastName);
        result.Value.Email.Should().Be(request.Email);
        result.Value.PhoneNumber.Should().Be(request.PhoneNumber);
        result.Value.DateOfBirth.Should().Be(request.DateOfBirth);

        var profile = await _dbContext.Set<UserProfile>().FirstOrDefaultAsync(p => p.UserId == _userId, TestContext.Current.CancellationToken);
        profile.Should().NotBeNull();
        profile.FirstName.Should().Be(request.FirstName);
        profile.LastName.Should().Be(request.LastName);
        profile.Email.Should().Be(request.Email);
    }

    [Fact(DisplayName = "Handle: Should create profile when DateOfBirth is null")]
    public async Task Handle_ShouldCreateProfile_WhenDateOfBirthIsNull()
    {
        var request = CreateValidRequest(dateOfBirth: null);

        var result = await _handler.Handle(new CreateProfile.Command(_userId, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.DateOfBirth.Should().BeNull();

        var profile = await _dbContext.Set<UserProfile>().FirstOrDefaultAsync(p => p.UserId == _userId, TestContext.Current.CancellationToken);
        profile!.DateOfBirth.Should().BeNull();
    }

    [Fact(DisplayName = "Handle: Should create profile when PhoneNumber is empty")]
    public async Task Handle_ShouldCreateProfile_WhenPhoneNumberIsEmpty()
    {
        var request = CreateValidRequest(phoneNumber: string.Empty, dateOfBirth: new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero));

        var result = await _handler.Handle(new CreateProfile.Command(_userId, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.PhoneNumber.Should().BeEmpty();

        var profile = await _dbContext.Set<UserProfile>().FirstOrDefaultAsync(p => p.UserId == _userId, TestContext.Current.CancellationToken);
        profile!.PhoneNumber.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handle: Should return UserNotFound when user does not exist")]
    public async Task Handle_ShouldReturnUserNotFound_WhenUserDoesNotExist()
    {
        var nonExistentUserId = Guid.NewGuid();
        var request = CreateValidRequest();

        var result = await _handler.Handle(new CreateProfile.Command(nonExistentUserId, request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserProfileResult.Failure.UserNotFound.Code);
    }

    [Fact(DisplayName = "Handle: Should return ProfileAlreadyExists when profile already exists")]
    public async Task Handle_ShouldReturnProfileAlreadyExists_WhenExists()
    {
        var existingProfile = ProfileUserFactory.Create(_userId);
        _dbContext.Set<UserProfile>().Add(existingProfile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateValidRequest();

        var result = await _handler.Handle(new CreateProfile.Command(_userId, request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserProfileResult.Failure.AlreadyExists.Code);
    }
}
