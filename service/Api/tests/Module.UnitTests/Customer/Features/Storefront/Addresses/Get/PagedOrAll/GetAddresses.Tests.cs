using Module.Customer.Domain;
using Module.Customer.Domain.Addresses;
using Module.Customer.Features.Storefront.Addresses.Get.PagedOrAll;
using Module.UnitTests.Profile.Domain;

namespace Module.UnitTests.Profile.Features.Store.Addresses.Get.PagedOrAll;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "AddressGetAll")]
public class GetAddressesTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetAddresses.PagedQueryHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public GetAddressesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new GetAddresses.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should return all addresses when no filter")]
    public async Task Handle_ShouldReturnAllAddresses_WhenNoFilter()
    {
        var profile = ProfileUserFactory.Create(_userId);
        var addr1 = AddressMethod.Create("John", "123 Main St", "New York", "USA").Value;
        var addr2 = AddressMethod.Create("John", "456 Oak Ave", "Los Angeles", "USA").Value;
        profile.AddAddress(addr1);
        profile.AddAddress(addr2);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetAddresses.Query(new GetAddresses.Parameters { UserId = _userId }), TestContext.Current.CancellationToken);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Should().OnlyContain(i => i.UserId == _userId);
    }

    [Fact(DisplayName = "Handle: Should return empty when profile doesn't exist")]
    public async Task Handle_ShouldReturnEmpty_WhenProfileMissing()
    {
        var result = await _handler.Handle(
            new GetAddresses.Query(new GetAddresses.Parameters { UserId = _userId }), TestContext.Current.CancellationToken);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact(DisplayName = "Handle: Should return empty list when no addresses")]
    public async Task Handle_ShouldReturnEmptyList_WhenNoAddresses()
    {
        var profile = ProfileUserFactory.Create(_userId);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetAddresses.Query(new GetAddresses.Parameters { UserId = _userId }), TestContext.Current.CancellationToken);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact(DisplayName = "Handle: Should handle pagination correctly")]
    public async Task Handle_ShouldHandlePagination()
    {
        var profile = ProfileUserFactory.Create(_userId);
        for (int i = 0; i < 5; i++)
        {
            var address = AddressMethod.Create("John", $"{i} St", "City", "Country").Value;
            profile.AddAddress(address);
        }
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetAddresses.Query(new GetAddresses.Parameters { UserId = _userId, PageNumber = 1, PageSize = 2 }), TestContext.Current.CancellationToken);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);
    }

    [Fact(DisplayName = "Handle: Should return second page correctly")]
    public async Task Handle_ShouldReturnSecondPage()
    {
        var profile = ProfileUserFactory.Create(_userId);
        for (int i = 0; i < 5; i++)
        {
            var address = AddressMethod.Create("John", $"{i} St", "City", "Country").Value;
            profile.AddAddress(address);
        }
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetAddresses.Query(new GetAddresses.Parameters { UserId = _userId, PageNumber = 2, PageSize = 2 }), TestContext.Current.CancellationToken);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(2);
    }
}
