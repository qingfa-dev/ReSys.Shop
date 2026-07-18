using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Admin.Addresses.Delete;
using Module.UnitTests.Profile.Domain;

namespace Module.UnitTests.Profile.Features.Admin.Addresses.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Profile")]
[Trait("Feature", "AdminAddressDelete")]
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
        var profile = ProfileUserFactory.Create(_userId);
        var address = AddressMethod.Create("John", "Delete St", "City", "Country").Value;
        profile.AddAddress(address);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new DeleteAddress.Command(address.Id, _userId), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(address.Id);

        var updated = await _dbContext.Set<UserProfile>().FirstAsync(p => p.UserId == _userId, TestContext.Current.CancellationToken);
        updated.Addresses.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handle: Should return NotFound if profile does not exist")]
    public async Task Handle_ShouldFail_WhenProfileNotFound()
    {
        var result = await _handler.Handle(new DeleteAddress.Command(Guid.NewGuid(), _userId), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserProfileResult.Failure.UserNotFound.Code);
    }

    [Fact(DisplayName = "Handle: Should return NotFound if address does not exist")]
    public async Task Handle_ShouldFail_WhenAddressNotFound()
    {
        var profile = ProfileUserFactory.Create(_userId);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new DeleteAddress.Command(Guid.NewGuid(), _userId), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(AddressResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Handle: Should promote new default if deleted address was default")]
    public async Task Handle_ShouldPromoteNewDefault_WhenDefaultDeleted()
    {
        var profile = ProfileUserFactory.Create(_userId);
        var addr1 = AddressMethod.Create("John", "Default St", "City", "Country", isDefault: true, addressType: AddressType.Shipping).Value;
        var addr2 = AddressMethod.Create("John", "Other St", "City", "Country", isDefault: false, addressType: AddressType.Shipping).Value;
        profile.AddAddress(addr1);
        profile.AddAddress(addr2);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new DeleteAddress.Command(addr1.Id, _userId), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var updated = await _dbContext.Set<UserProfile>().FirstAsync(p => p.UserId == _userId, TestContext.Current.CancellationToken);
        updated.Addresses.First(a => a.Id == addr2.Id).IsDefault.Should().BeTrue();
    }
}
