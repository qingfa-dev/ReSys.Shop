using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Admin.Addresses.Get.ById;
using Module.UnitTests.Profile.Domain;

using Address = Module.Profile.Domain.Addresses.Address;

namespace Module.UnitTests.Profile.Features.Admin.Addresses.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Profile")]
[Trait("Feature", "AdminAddressGetById")]
public class GetAddressByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetAddressById.QueryHandler _handler;

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

    [Fact(DisplayName = "Handle: Should return address by Id")]
    public async Task Handle_ShouldReturnAddress()
    {
        var address = AddressMethod.Create("John", "St", "City", "Country").Value;
        _dbContext.Set<Address>().Add(address);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetAddressById.Query(address.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(address.Id);
        result.Value.Address1.Should().Be("St");
    }

    [Fact(DisplayName = "Handle: Should return NotFound if address does not exist")]
    public async Task Handle_ShouldFail_WhenNotFound()
    {
        var result = await _handler.Handle(new GetAddressById.Query(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(AddressResult.Failure.NotFound.Code);
    }
}
