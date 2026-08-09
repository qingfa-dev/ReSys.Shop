using Module.Customer.Domain;
using Module.Customer.Domain.Addresses;
using Module.Customer.Features.Storefront.Addresses.Create;
using Module.UnitTests.Profile.Domain;

using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Profile.Features.Store.Addresses.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "AddressCreate")]
public class CreateAddressTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CreateAddress.CommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public CreateAddressTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new CreateAddress.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private static CreateAddress.Request CreateValidRequest(
        AddressType type = AddressType.Shipping,
        string address1 = "123 Main St",
        bool isDefault = false)
    {
        return new CreateAddress.Request
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

    [Fact(DisplayName = "Handle: Should create first address as default automatically")]
    public async Task Handle_ShouldCreateFirstAddressAsDefault()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateValidRequest(isDefault: false);

        // Act
        var result = await _handler.Handle(new CreateAddress.Command(_userId, request), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsDefault.Should().BeTrue();
        
        var updatedProfile = await _dbContext.Set<UserProfile>().FirstAsync(p => p.UserId == _userId, TestContext.Current.CancellationToken);
        updatedProfile.Addresses.Should().HaveCount(1);
        updatedProfile.Addresses.First().IsDefault.Should().BeTrue();
    }

    [Fact(DisplayName = "Handle: Should return NotFound if profile does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenProfileMissing()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = await _handler.Handle(new CreateAddress.Command(_userId, request), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Handle: Should fail if total address limit is reached")]
    public async Task Handle_ShouldFail_WhenTotalLimitReached()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        var types = new[] { AddressType.Shipping, AddressType.Billing, AddressType.Other };
        for (int i = 0; i < UserProfileConstant.Constraints.MaxAddressesCount; i++)
        {
            var type = types[i % types.Length];
            profile.AddAddress(AddressMethod.Create($"User{i}", $"{i} St", "City", "Country", addressType: type).Value);
        }
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateValidRequest(address1: "New St");

        // Act
        var result = await _handler.Handle(new CreateAddress.Command(_userId, request), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(AddressResult.Failure.MaxAddressesReached.Code);
    }

    [Fact(DisplayName = "Handle: Should fail if per-type address limit is reached")]
    public async Task Handle_ShouldFail_WhenPerTypeLimitReached()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        for (int i = 0; i < UserProfileConstant.Constraints.MaxAddressesCountPerType; i++)
        {
            profile.AddAddress(AddressMethod.Create("John", $"{i} St", "City", "Country", addressType: AddressType.Shipping).Value);
        }
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateValidRequest(type: AddressType.Shipping, address1: "New St");

        // Act
        var result = await _handler.Handle(new CreateAddress.Command(_userId, request), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(AddressResult.Failure.MaxAddressesPerTypeReached.Code);
    }

    [Fact(DisplayName = "Handle: Should fail if address is duplicate")]
    public async Task Handle_ShouldFail_WhenDuplicateFound()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        var existing = AddressMethod.Create("John", "123 Main St", "New York", "USA", zipCode: "10001").Value;
        profile.AddAddress(existing);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateValidRequest(); // Same as existing

        // Act
        var result = await _handler.Handle(new CreateAddress.Command(_userId, request), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(AddressResult.Failure.DuplicateAddress.Code);
    }

    [Fact(DisplayName = "Handle: Should unset other defaults of same type when new address is default")]
    public async Task Handle_ShouldUnsetOtherDefaults_WhenNewIsDefault()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        var existing = AddressMethod.Create("John", "Old St", "City", "Country", isDefault: true, addressType: AddressType.Shipping).Value;
        profile.AddAddress(existing);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = CreateValidRequest(type: AddressType.Shipping, address1: "New St", isDefault: true);

        // Act
        var result = await _handler.Handle(new CreateAddress.Command(_userId, request), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var updatedProfile = await _dbContext.Set<UserProfile>().FirstAsync(p => p.UserId == _userId, TestContext.Current.CancellationToken);
        updatedProfile.Addresses.First(a => a.Address1 == "New St").IsDefault.Should().BeTrue();
        updatedProfile.Addresses.First(a => a.Address1 == "Old St").IsDefault.Should().BeFalse();
    }
}
