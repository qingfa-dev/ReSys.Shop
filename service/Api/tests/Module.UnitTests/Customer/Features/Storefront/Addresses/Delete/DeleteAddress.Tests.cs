using Module.Customer.Domain;
using Module.Customer.Domain.Addresses;
using Module.Customer.Features.Storefront.Addresses.Delete;
using Module.UnitTests.Profile.Domain;

using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Profile.Features.Store.Addresses.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "AddressDelete")]
public class DeleteAddressTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DeleteAddress.CommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public DeleteAddressTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new DeleteAddress.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should delete address successfully")]
    public async Task Handle_ShouldDeleteAddress()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        var address = AddressMethod.Create("John", "Delete St", "City", "Country").Value;
        profile.AddAddress(address);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(new DeleteAddress.Command(_userId, address.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(address.Id);
        
        var updatedProfile = await _dbContext.Set<UserProfile>().FirstAsync(p => p.UserId == _userId, TestContext.Current.CancellationToken);
        updatedProfile.Addresses.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handle: Should return NotFound if address does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenAddressMissing()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(new DeleteAddress.Command(_userId, Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(AddressResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Handle: Should promote new default if deleted address was default")]
    public async Task Handle_ShouldPromoteNewDefault_WhenDefaultDeleted()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        var addr1 = AddressMethod.Create("John", "Default St", "City", "Country", isDefault: true, addressType: AddressType.Shipping).Value;
        var addr2 = AddressMethod.Create("John", "Other St", "City", "Country", isDefault: false, addressType: AddressType.Shipping).Value;
        profile.AddAddress(addr1);
        profile.AddAddress(addr2);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(new DeleteAddress.Command(_userId, addr1.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var updatedProfile = await _dbContext.Set<UserProfile>().FirstAsync(p => p.UserId == _userId, TestContext.Current.CancellationToken);
        updatedProfile.Addresses.Should().HaveCount(1);
        updatedProfile.Addresses.First().Id.Should().Be(addr2.Id);
        updatedProfile.Addresses.First().IsDefault.Should().BeTrue(); // Promoted
    }

    [Fact(DisplayName = "Handle: Should return NotFound if profile doesn't exist")]
    public async Task Handle_ShouldReturnNotFound_WhenProfileMissing()
    {
        // Act
        var result = await _handler.Handle(new DeleteAddress.Command(_userId, Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Handle: Should return response with address label when available")]
    public async Task Handle_ShouldReturnResponseWithLabel_WhenLabelExists()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        var address = AddressMethod.Create("John", "Main St", "City", "Country", label: "Home").Value;
        profile.AddAddress(address);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(new DeleteAddress.Command(_userId, address.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Label.Should().Be("Home");
    }
}
