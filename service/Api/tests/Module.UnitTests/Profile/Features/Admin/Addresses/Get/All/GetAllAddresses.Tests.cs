using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Admin.Addresses.Get.All;
using Module.UnitTests.Profile.Domain;

namespace Module.UnitTests.Profile.Features.Admin.Addresses.Get.All;

[Trait("Category", "Unit")]
[Trait("Module", "Profile")]
[Trait("Feature", "AdminAddressGetAll")]
public class GetAllAddressesTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetAllAddresses.QueryHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public GetAllAddressesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new GetAllAddresses.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should return all addresses for user")]
    public async Task Handle_ShouldReturnAllAddresses()
    {
        var profile = ProfileUserFactory.Create(_userId);
        var addr1 = AddressMethod.Create("John", "St 1", "City", "Country", addressType: AddressType.Shipping).Value;
        var addr2 = AddressMethod.Create("John", "St 2", "City", "Country", addressType: AddressType.Billing).Value;
        profile.AddAddress(addr1);
        profile.AddAddress(addr2);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetAllAddresses.Query(_userId), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Handle: Should return NotFound if profile does not exist")]
    public async Task Handle_ShouldFail_WhenProfileNotFound()
    {
        var result = await _handler.Handle(new GetAllAddresses.Query(_userId), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserProfileResult.Failure.UserNotFound.Code);
    }

    [Fact(DisplayName = "Handle: Should return empty list when no addresses")]
    public async Task Handle_ShouldReturnEmpty_WhenNoAddresses()
    {
        var profile = ProfileUserFactory.Create(_userId);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetAllAddresses.Query(_userId), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
