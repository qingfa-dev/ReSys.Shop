using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Store.Addresses.Update;
using Module.UnitTests.Identity.Fixtures;
using Module.UnitTests.Profile.Domain;

using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Profile.Features.Store.Addresses.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "AddressUpdate")]
public class UpdateAddressTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly UpdateAddress.CommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public UpdateAddressTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _currentUserMock = IdentityMocks.CreateCurrentUserMock(_userId);
        
        _handler = new UpdateAddress.CommandHandler(_dbContext, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private static UpdateAddress.Request CreateValidRequest(
        AddressType type = AddressType.Shipping,
        string address1 = "Updated St",
        bool isDefault = false)
    {
        return new UpdateAddress.Request
        {
            AddressType = type,
            FirstName = "John",
            Address1 = address1,
            City = "New York",
            CountryName = "USA",
            ZipCode = "10001",
            IsDefault = isDefault
        };
    }

    [Fact(DisplayName = "Handle: Should update address details successfully")]
    public async Task Handle_ShouldUpdateAddressDetails()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        var address = AddressMethod.Create("John", "Old St", "City", "Country").Value;
        profile.AddAddress(address);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateValidRequest(address1: "New St");

        // Act
        var result = await _handler.Handle(new UpdateAddress.Command(address.Id, request), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Address1.Should().Be("New St");
        
        var updatedProfile = await _dbContext.Set<UserProfile>().FirstAsync(p => p.UserId == _userId, TestContext.Current.CancellationToken);
        updatedProfile.Addresses.First().Address1.Should().Be("New St");
    }

    [Fact(DisplayName = "Handle: Should return Unauthorized if user is not authenticated")]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        _currentUserMock.Setup(x => x.UserId).Returns((string?)null);
        var request = CreateValidRequest();

        // Act
        var result = await _handler.Handle(new UpdateAddress.Command(Guid.NewGuid(), request), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(401);
    }

    [Fact(DisplayName = "Handle: Should fail if address is duplicate of another")]
    public async Task Handle_ShouldFail_WhenDuplicateFound()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        var address1 = AddressMethod.Create("John", "123 Main St", "New York", "USA", zipCode: "10001").Value;
        var address2 = AddressMethod.Create("John", "456 Other St", "New York", "USA", zipCode: "10001").Value;
        profile.AddAddress(address1);
        profile.AddAddress(address2);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateValidRequest(address1: "456 Other St"); // Match address2 exactly
        // request also has City: "New York", CountryName: "USA", ZipCode: "10001" by default in helper

        // Act
        var result = await _handler.Handle(new UpdateAddress.Command(address1.Id, request), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(AddressResult.Failure.DuplicateAddress.Code);
    }

    [Fact(DisplayName = "Handle: Should fail if per-type limit reached when changing type")]
    public async Task Handle_ShouldFail_WhenNewTypeLimitReached()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        // Add 5 Shipping addresses
        for (int i = 0; i < UserProfileConstant.Constraints.MaxAddressesCountPerType; i++)
        {
            profile.AddAddress(AddressMethod.Create("John", $"Shipping {i}", "City", "Country", addressType: AddressType.Shipping).Value);
        }
        // Add 1 Billing address
        var billingAddress = AddressMethod.Create("John", "Billing St", "City", "Country", addressType: AddressType.Billing).Value;
        profile.AddAddress(billingAddress);
        
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateValidRequest(type: AddressType.Shipping); // Try to change Billing to Shipping

        // Act
        var result = await _handler.Handle(new UpdateAddress.Command(billingAddress.Id, request), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(AddressResult.Failure.MaxAddressesPerTypeReached.Code);
    }

    [Fact(DisplayName = "Handle: Should ensure old type still has a default if type changed")]
    public async Task Handle_ShouldEnsureOldTypeHasDefault_WhenTypeChanges()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        var addr1 = AddressMethod.Create("John", "St 1", "City", "Country", addressType: AddressType.Shipping, isDefault: true).Value;
        var addr2 = AddressMethod.Create("John", "St 2", "City", "Country", addressType: AddressType.Shipping, isDefault: false).Value;
        profile.AddAddress(addr1);
        profile.AddAddress(addr2);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateValidRequest(type: AddressType.Billing, isDefault: true); // Move addr1 to Billing

        // Act
        var result = await _handler.Handle(new UpdateAddress.Command(addr1.Id, request), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var updatedProfile = await _dbContext.Set<UserProfile>().FirstAsync(p => p.UserId == _userId, TestContext.Current.CancellationToken);
        updatedProfile.Addresses.First(a => a.Id == addr1.Id).AddressType.Should().Be(AddressType.Billing);
        updatedProfile.Addresses.First(a => a.Id == addr1.Id).IsDefault.Should().BeTrue();
        updatedProfile.Addresses.First(a => a.Id == addr2.Id).IsDefault.Should().BeTrue(); // Should have been promoted
    }

    [Fact(DisplayName = "Handle: Should return NotFound if profile doesn't exist")]
    public async Task Handle_ShouldReturnNotFound_WhenProfileMissing()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = await _handler.Handle(new UpdateAddress.Command(Guid.NewGuid(), request), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Handle: Should set address as default when it's the only one of type")]
    public async Task Handle_ShouldSetAsDefault_WhenOnlyOneOfType()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        var address = AddressMethod.Create("John", "St", "City", "Country", isDefault: false, addressType: AddressType.Shipping).Value;
        profile.AddAddress(address);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateValidRequest(isDefault: false);

        // Act
        var result = await _handler.Handle(new UpdateAddress.Command(address.Id, request), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsDefault.Should().BeTrue(); // Must be default since it's the only one
    }

    [Fact(DisplayName = "Handle: Should not affect other defaults when updating non-default address")]
    public async Task Handle_ShouldNotAffectOtherDefaults_WhenUpdatingNonDefault()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        var addr1 = AddressMethod.Create("John", "Default St", "City", "Country", isDefault: true, addressType: AddressType.Shipping).Value;
        var addr2 = AddressMethod.Create("John", "Other St", "City", "Country", isDefault: false, addressType: AddressType.Shipping).Value;
        profile.AddAddress(addr1);
        profile.AddAddress(addr2);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateValidRequest(isDefault: false); // Don't change default status

        // Act
        var result = await _handler.Handle(new UpdateAddress.Command(addr2.Id, request), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsDefault.Should().BeFalse();
        
        var updatedProfile = await _dbContext.Set<UserProfile>().FirstAsync(p => p.UserId == _userId, TestContext.Current.CancellationToken);
        updatedProfile.Addresses.First(a => a.Id == addr1.Id).IsDefault.Should().BeTrue(); // Should remain default
    }

    [Fact(DisplayName = "Handle: Should update all address fields correctly")]
    public async Task Handle_ShouldUpdateAllFields()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        var address = AddressMethod.Create("John", "Old St", "Old City", "Old Country").Value;
        profile.AddAddress(address);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateAddress.Request
        {
            AddressType = AddressType.Billing,
            FirstName = "Jane",
            Address1 = "New St",
            Address2 = "Apt 5",
            City = "Los Angeles",
            CountryName = "Canada",
            ZipCode = "90210",
            Phone = "+1234567890",
            Label = "Work",
            IsDefault = true
        };

        // Act
        var result = await _handler.Handle(new UpdateAddress.Command(address.Id, request), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be("Jane");
        result.Value.Address1.Should().Be("New St");
        result.Value.Address2.Should().Be("Apt 5");
        result.Value.City.Should().Be("Los Angeles");
        result.Value.CountryName.Should().Be("Canada");
        result.Value.ZipCode.Should().Be("90210");
        result.Value.Phone.Should().Be("+1234567890");
        result.Value.Label.Should().Be("Work");
        result.Value.AddressType.Should().Be(AddressType.Billing);
    }
}
