using Module.Location.Domain.Countries;
using Module.Location.Features.Admin.Countries.GetByIsoCode;

namespace Module.UnitTests.Location.Features.Admin.Countries.Get.ByIsoCode;

[Trait("Category", "Unit")]
[Trait("Module", "Location")]
[Trait("Feature", "CountryGetByIsoCode")]
public class GetCountryByIsoTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetCountryByIso.QueryHandler _handler;

    public GetCountryByIsoTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Country).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new GetCountryByIso.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Should return country when ISO code exists")]
    public async Task Handle_ShouldReturnCountry_WhenIsoCodeExists()
    {
        // Arrange
        var country = new Country { Name = "United States", IsoCode = "US", CallingCode = "+1" };
        _dbContext.Set<Country>().Add(country);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new GetCountryByIso.Query("US"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.IsoCode.Should().Be("US");
        result.Value.Name.Should().Be("United States");
    }

    [Fact(DisplayName = "Should return NotFound when country doesn't exist")]
    public async Task Handle_ShouldReturnNotFound_WhenCountryNotFound()
    {
        // Arrange
        // No country in database

        // Act
        var result = await _handler.Handle(
            new GetCountryByIso.Query("XX"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(CountryResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Should return correct country when multiple exist")]
    public async Task Handle_ShouldReturnCorrectCountry_FromMultiple()
    {
        // Arrange
        _dbContext.Set<Country>().Add(new Country { Name = "United States", IsoCode = "US" });
        _dbContext.Set<Country>().Add(new Country { Name = "Canada", IsoCode = "CA" });
        _dbContext.Set<Country>().Add(new Country { Name = "Mexico", IsoCode = "MX" });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new GetCountryByIso.Query("CA"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Canada");
    }
}