using Microsoft.EntityFrameworkCore;

using Module.Location.Domain.Countries;
using Module.Location.Features.Admin.Countries.GetById;

using Shared.Operational.Persistence.Data;

namespace Module.UnitTests.Location.Features.Admin.Countries.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Locations")]
[Trait("Feature", "CountryGetById")]
public class GetCountryByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetCountryById.QueryHandler _handler;

    public GetCountryByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Country).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new GetCountryById.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Should return country when Id exists")]
    public async Task Handle_ShouldReturnCountry_WhenIdExists()
    {
        // Arrange
        var country = new Country { Name = "United States", IsoCode = "US", CallingCode = "+1" };
        _dbContext.Set<Country>().Add(country);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new GetCountryById.Query(country.Id),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.IsoCode.Should().Be("US");
    }

    [Fact(DisplayName = "Should return NotFound when Id doesn't exist")]
    public async Task Handle_ShouldReturnNotFound_WhenIdNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _handler.Handle(
            new GetCountryById.Query(nonExistentId),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(CountryResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "Should return correct country from multiple")]
    public async Task Handle_ShouldReturnCorrectCountry_FromMultiple()
    {
        // Arrange
        var country1 = new Country { Name = "United States", IsoCode = "US" };
        var country2 = new Country { Name = "Canada", IsoCode = "CA" };
        var country3 = new Country { Name = "Mexico", IsoCode = "MX" };
        _dbContext.Set<Country>().Add(country1);
        _dbContext.Set<Country>().Add(country2);
        _dbContext.Set<Country>().Add(country3);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new GetCountryById.Query(country2.Id),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Canada");
    }
}