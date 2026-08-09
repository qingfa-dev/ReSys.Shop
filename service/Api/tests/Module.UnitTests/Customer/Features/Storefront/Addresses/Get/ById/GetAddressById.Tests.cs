using Module.Customer.Domain;
using Module.Customer.Domain.Addresses;
using Module.Customer.Features.Storefront.Addresses.Get.ById;
using Module.UnitTests.Profile.Domain;

using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Profile.Features.Store.Addresses.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "AddressGetById")]
public class GetAddressByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetAddressById.QueryHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public GetAddressByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new GetAddressById.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should return address by ID successfully")]
    public async Task Handle_ShouldReturnAddressById()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        var address = AddressMethod.Create("John", "123 Main St", "New York", "USA", zipCode: "10001").Value;
        profile.AddAddress(address);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(new GetAddressById.Query(_userId, address.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(address.Id);
        result.Value.Address1.Should().Be("123 Main St");
        result.Value.City.Should().Be("New York");
        result.Value.CountryName.Should().Be("USA");
    }

    [Fact(DisplayName = "Handle: Should return NotFound when profile doesn't exist")]
    public async Task Handle_ShouldReturnNotFound_WhenProfileMissing()
    {
        // Act
        var result = await _handler.Handle(new GetAddressById.Query(_userId, Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Handle: Should return NotFound when address doesn't exist")]
    public async Task Handle_ShouldReturnNotFound_WhenAddressMissing()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(new GetAddressById.Query(_userId, Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(AddressResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Handle: Should return address with all fields")]
    public async Task Handle_ShouldReturnAddressWithAllFields()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        var address = AddressMethod.Create("John", "123 Main St", "New York", "USA",
                addressType: AddressType.Billing,
                isDefault: true,
                address2: "Apt 5B",
                zipCode: "10001",
                phone: "+1234567890",
                label: "Work")
            .Value;
        profile.AddAddress(address);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(new GetAddressById.Query(_userId, address.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AddressType.Should().Be(AddressType.Billing);
        result.Value.IsDefault.Should().BeTrue();
        result.Value.Address2.Should().Be("Apt 5B");
        result.Value.ZipCode.Should().Be("10001");
        result.Value.Phone.Should().Be("+1234567890");
        result.Value.Label.Should().Be("Work");
    }
}
