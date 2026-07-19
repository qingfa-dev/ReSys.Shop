using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Admin.Addresses.Create;
using Module.UnitTests.Profile.Domain;

namespace Module.UnitTests.Profile.Features.Admin.Addresses.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Profile")]
[Trait("Feature", "AdminAddressCreate")]
public class CreateUserAddressTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CreateUserAddress.CommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public CreateUserAddressTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new CreateUserAddress.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private static CreateUserAddress.Request CreateRequest(
        Guid userId,
        AddressType type = AddressType.Shipping,
        string address1 = "123 Main St",
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

    [Fact(DisplayName = "Handle: Should create first address as default automatically")]
    public async Task Handle_ShouldCreateFirstAddressAsDefault()
    {
        var profile = ProfileUserFactory.Create(_userId);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateRequest(_userId, address1: "New St", isDefault: false);

        var result = await _handler.Handle(new CreateUserAddress.Command(request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsDefault.Should().BeTrue();

        var updated = await _dbContext.Set<UserProfile>().FirstAsync(p => p.UserId == _userId, TestContext.Current.CancellationToken);
        updated.Addresses.Should().HaveCount(1);
        updated.Addresses.First().IsDefault.Should().BeTrue();
    }

    [Fact(DisplayName = "Handle: Should return NotFound if profile does not exist")]
    public async Task Handle_ShouldFail_WhenProfileNotFound()
    {
        var request = CreateRequest(Guid.NewGuid());

        var result = await _handler.Handle(new CreateUserAddress.Command(request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserProfileResult.Failure.UserNotFound.Code);
    }

    [Fact(DisplayName = "Handle: Should fail if total address limit is reached")]
    public async Task Handle_ShouldFail_WhenTotalLimitReached()
    {
        var profile = ProfileUserFactory.Create(_userId);
        var types = new[] { AddressType.Shipping, AddressType.Billing, AddressType.Other };
        for (int i = 0; i < UserProfileConstant.Constraints.MaxAddressesCount; i++)
        {
            var type = types[i % types.Length];
            profile.AddAddress(AddressMethod.Create($"User{i}", $"{i} St", "City", "Country", addressType: type).Value);
        }
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateRequest(_userId, address1: "New St");

        var result = await _handler.Handle(new CreateUserAddress.Command(request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(AddressResult.Failure.MaxAddressesReached.Code);
    }

    [Fact(DisplayName = "Handle: Should fail if per-type address limit is reached")]
    public async Task Handle_ShouldFail_WhenPerTypeLimitReached()
    {
        var profile = ProfileUserFactory.Create(_userId);
        for (int i = 0; i < UserProfileConstant.Constraints.MaxAddressesCountPerType; i++)
        {
            profile.AddAddress(AddressMethod.Create("John", $"{i} St", "City", "Country", addressType: AddressType.Shipping).Value);
        }
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateRequest(_userId, type: AddressType.Shipping, address1: "New St");

        var result = await _handler.Handle(new CreateUserAddress.Command(request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(AddressResult.Failure.MaxAddressesPerTypeReached.Code);
    }

    [Fact(DisplayName = "Handle: Should fail if address is duplicate")]
    public async Task Handle_ShouldFail_WhenDuplicateFound()
    {
        var profile = ProfileUserFactory.Create(_userId);
        var existing = AddressMethod.Create("John", "123 Main St", "New York", "USA", zipCode: "10001").Value;
        profile.AddAddress(existing);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateRequest(_userId);

        var result = await _handler.Handle(new CreateUserAddress.Command(request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(AddressResult.Failure.DuplicateAddress.Code);
    }

    [Fact(DisplayName = "Handle: Should unset other defaults of same type when new address is default")]
    public async Task Handle_ShouldUnsetOtherDefaults_WhenNewIsDefault()
    {
        var profile = ProfileUserFactory.Create(_userId);
        var existing = AddressMethod.Create("John", "Old St", "City", "Country", isDefault: true, addressType: AddressType.Shipping).Value;
        profile.AddAddress(existing);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateRequest(_userId, type: AddressType.Shipping, address1: "New St", isDefault: true);

        var result = await _handler.Handle(new CreateUserAddress.Command(request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var updated = await _dbContext.Set<UserProfile>().FirstAsync(p => p.UserId == _userId, TestContext.Current.CancellationToken);
        updated.Addresses.First(a => a.Address1 == "New St").IsDefault.Should().BeTrue();
        updated.Addresses.First(a => a.Address1 == "Old St").IsDefault.Should().BeFalse();
    }
}
