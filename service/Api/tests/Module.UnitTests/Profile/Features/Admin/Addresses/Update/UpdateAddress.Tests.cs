using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Admin.Addresses.Update;
using Module.UnitTests.Profile.Domain;

namespace Module.UnitTests.Profile.Features.Admin.Addresses.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Profile")]
[Trait("Feature", "AdminAddressUpdate")]
public class UpdateAddressTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UpdateAddress.CommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public UpdateAddressTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new UpdateAddress.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private static UpdateAddress.Request CreateRequest(
        Guid userId,
        AddressType type = AddressType.Shipping,
        string address1 = "Updated St",
        bool isDefault = false) => new()
    {
        UserId = userId,
        AddressType = type,
        FirstName = "John",
        Address1 = address1,
        City = "New York",
        CountryName = "USA",
        ZipCode = "10001",
        IsDefault = isDefault
    };

    [Fact(DisplayName = "Handle: Should update address details successfully")]
    public async Task Handle_ShouldUpdateAddressDetails()
    {
        var profile = ProfileUserFactory.Create(_userId);
        var address = AddressMethod.Create("John", "Old St", "City", "Country").Value;
        profile.AddAddress(address);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateRequest(_userId, address1: "New St");

        var result = await _handler.Handle(new UpdateAddress.Command(address.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Address1.Should().Be("New St");

        var updated = await _dbContext.Set<UserProfile>().FirstAsync(p => p.UserId == _userId, TestContext.Current.CancellationToken);
        updated.Addresses.First().Address1.Should().Be("New St");
    }

    [Fact(DisplayName = "Handle: Should return NotFound if profile does not exist")]
    public async Task Handle_ShouldFail_WhenProfileNotFound()
    {
        var request = CreateRequest(Guid.NewGuid());
        var result = await _handler.Handle(new UpdateAddress.Command(Guid.NewGuid(), request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserProfileResult.Failure.UserNotFound.Code);
    }

    [Fact(DisplayName = "Handle: Should return NotFound if address does not exist")]
    public async Task Handle_ShouldFail_WhenAddressNotFound()
    {
        var profile = ProfileUserFactory.Create(_userId);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateRequest(_userId);

        var result = await _handler.Handle(new UpdateAddress.Command(Guid.NewGuid(), request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(AddressResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Handle: Should fail if address is duplicate of another")]
    public async Task Handle_ShouldFail_WhenDuplicateFound()
    {
        var profile = ProfileUserFactory.Create(_userId);
        var addr1 = AddressMethod.Create("John", "123 Main St", "New York", "USA", zipCode: "10001").Value;
        var addr2 = AddressMethod.Create("John", "456 Other St", "New York", "USA", zipCode: "10001").Value;
        profile.AddAddress(addr1);
        profile.AddAddress(addr2);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateRequest(_userId, address1: "456 Other St");

        var result = await _handler.Handle(new UpdateAddress.Command(addr1.Id, request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(AddressResult.Failure.DuplicateAddress.Code);
    }

    [Fact(DisplayName = "Handle: Should fail if per-type limit reached when changing type")]
    public async Task Handle_ShouldFail_WhenNewTypeLimitReached()
    {
        var profile = ProfileUserFactory.Create(_userId);
        for (int i = 0; i < UserProfileConstant.Constraints.MaxAddressesCountPerType; i++)
            profile.AddAddress(AddressMethod.Create("John", $"Shipping {i}", "City", "Country", addressType: AddressType.Shipping).Value);
        var billing = AddressMethod.Create("John", "Billing St", "City", "Country", addressType: AddressType.Billing).Value;
        profile.AddAddress(billing);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateRequest(_userId, type: AddressType.Shipping);

        var result = await _handler.Handle(new UpdateAddress.Command(billing.Id, request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(AddressResult.Failure.MaxAddressesPerTypeReached.Code);
    }

    [Fact(DisplayName = "Handle: Should ensure old type still has a default if type changed")]
    public async Task Handle_ShouldEnsureOldTypeHasDefault_WhenTypeChanges()
    {
        var profile = ProfileUserFactory.Create(_userId);
        var addr1 = AddressMethod.Create("John", "St 1", "City", "Country", addressType: AddressType.Shipping, isDefault: true).Value;
        var addr2 = AddressMethod.Create("John", "St 2", "City", "Country", addressType: AddressType.Shipping, isDefault: false).Value;
        profile.AddAddress(addr1);
        profile.AddAddress(addr2);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateRequest(_userId, type: AddressType.Billing, isDefault: true);

        var result = await _handler.Handle(new UpdateAddress.Command(addr1.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var updated = await _dbContext.Set<UserProfile>().FirstAsync(p => p.UserId == _userId, TestContext.Current.CancellationToken);
        updated.Addresses.First(a => a.Id == addr1.Id).AddressType.Should().Be(AddressType.Billing);
        updated.Addresses.First(a => a.Id == addr1.Id).IsDefault.Should().BeTrue();
        updated.Addresses.First(a => a.Id == addr2.Id).IsDefault.Should().BeTrue();
    }

    [Fact(DisplayName = "Handle: Should set address as default when it's the only one of type")]
    public async Task Handle_ShouldSetAsDefault_WhenOnlyOneOfType()
    {
        var profile = ProfileUserFactory.Create(_userId);
        var address = AddressMethod.Create("John", "St", "City", "Country", isDefault: false, addressType: AddressType.Shipping).Value;
        profile.AddAddress(address);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateRequest(_userId, isDefault: false);

        var result = await _handler.Handle(new UpdateAddress.Command(address.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsDefault.Should().BeTrue();
    }

    [Fact(DisplayName = "Handle: Should update all address fields correctly")]
    public async Task Handle_ShouldUpdateAllFields()
    {
        var profile = ProfileUserFactory.Create(_userId);
        var address = AddressMethod.Create("John", "Old St", "Old City", "Old Country").Value;
        profile.AddAddress(address);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateAddress.Request
        {
            UserId = _userId,
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

        var result = await _handler.Handle(new UpdateAddress.Command(address.Id, request), TestContext.Current.CancellationToken);

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
