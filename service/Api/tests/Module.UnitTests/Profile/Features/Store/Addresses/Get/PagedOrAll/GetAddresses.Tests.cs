using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Store.Addresses.Get.PagedOrAll;
using Module.UnitTests.Identity.Fixtures;
using Module.UnitTests.Profile.Domain;

namespace Module.UnitTests.Profile.Features.Store.Addresses.Get.PagedOrAll;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "AddressGetAll")]
public class GetAddressesTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly GetAddresses.PagedQueryHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public GetAddressesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _currentUserMock = IdentityMocks.CreateCurrentUserMock(_userId);
        
        _handler = new GetAddresses.PagedQueryHandler(_dbContext, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should return all addresses when no filter")]
    public async Task Handle_ShouldReturnAllAddresses_WhenNoFilter()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        var addr1 = AddressMethod.Create("John", "123 Main St", "New York", "USA").Value;
        var addr2 = AddressMethod.Create("John", "456 Oak Ave", "Los Angeles", "USA").Value;
        profile.AddAddress(addr1);
        profile.AddAddress(addr2);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(new GetAddresses.Query(new GetAddresses.Parameters()), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact(DisplayName = "Handle: Should return Unauthorized when user not authenticated")]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        _currentUserMock.Setup(x => x.UserId).Returns((string?)null);

        // Act
        var result = await _handler.Handle(new GetAddresses.Query(new GetAddresses.Parameters()), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact(DisplayName = "Handle: Should return empty when profile doesn't exist")]
    public async Task Handle_ShouldReturnEmpty_WhenProfileMissing()
    {
        // Act
        var result = await _handler.Handle(new GetAddresses.Query(new GetAddresses.Parameters()), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact(DisplayName = "Handle: Should filter by address type")]
    public async Task Handle_ShouldFilterByAddressType()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        var shipping = AddressMethod.Create("John", "Ship St", "City", "Country", addressType: AddressType.Shipping).Value;
        var billing = AddressMethod.Create("John", "Bill St", "City", "Country", addressType: AddressType.Billing).Value;
        profile.AddAddress(shipping);
        profile.AddAddress(billing);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(new GetAddresses.Query(new GetAddresses.Parameters { AddressType = AddressType.Shipping }), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.ElementAt(0).AddressType.Should().Be(AddressType.Shipping);
        result.TotalCount.Should().Be(1);
    }

    [Fact(DisplayName = "Handle: Should return empty list when no addresses")]
    public async Task Handle_ShouldReturnEmptyList_WhenNoAddresses()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(new GetAddresses.Query(new GetAddresses.Parameters()), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact(DisplayName = "Handle: Should handle pagination correctly")]
    public async Task Handle_ShouldHandlePagination()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        for (int i = 0; i < 5; i++)
        {
            var address = AddressMethod.Create("John", $"{i} St", "City", "Country").Value;
            profile.AddAddress(address);
        }
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act - Page 1 with page size 2
        var result = await _handler.Handle(new GetAddresses.Query(new GetAddresses.Parameters { PageNumber = 1, PageSize = 2 }), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);
    }

    [Fact(DisplayName = "Handle: Should return second page correctly")]
    public async Task Handle_ShouldReturnSecondPage()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        for (int i = 0; i < 5; i++)
        {
            var address = AddressMethod.Create("John", $"{i} St", "City", "Country").Value;
            profile.AddAddress(address);
        }
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act - Page 2 with page size 2
        var result = await _handler.Handle(new GetAddresses.Query(new GetAddresses.Parameters { PageNumber = 2, PageSize = 2 }), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(2);
    }

    [Fact(DisplayName = "Handle: Should filter by address type with pagination")]
    public async Task Handle_ShouldFilterByTypeWithPagination()
    {
        // Arrange
        var profile = ProfileUserFactory.Create(_userId);
        for (int i = 0; i < 3; i++)
        {
            var shipping = AddressMethod.Create("John", $"Ship{i} St", "City", "Country", addressType: AddressType.Shipping).Value;
            var billing = AddressMethod.Create("John", $"Bill{i} St", "City", "Country", addressType: AddressType.Billing).Value;
            profile.AddAddress(shipping);
            profile.AddAddress(billing);
        }
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act - Get shipping addresses only, page 1
        var result = await _handler.Handle(new GetAddresses.Query(new GetAddresses.Parameters { AddressType = AddressType.Shipping, PageNumber = 1, PageSize = 2 }), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(a => a.AddressType.Should().Be(AddressType.Shipping));
        result.TotalCount.Should().Be(3);
    }
}
