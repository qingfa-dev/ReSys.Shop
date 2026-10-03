using Module.Location.Domain.Countries;
using Module.Location.Domain.States;
using Module.Location.Features.Shared.Queries;

namespace Module.UnitTests.Location.Features.Shared.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Location")]
[Trait("Feature", "ExistsByIso")]
public class ExistsByIsoQueryTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Guid _countryId = Guid.NewGuid();

    public ExistsByIsoQueryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Country).Assembly];
        _dbContext = new ApplicationDbContext(options);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "CountryExists: Should return true for existing ISO code regardless of case")]
    public async Task CountryExists_ShouldReturnTrue_WhenIsoCodeExistsIgnoringCase()
    {
        _dbContext.Set<Country>().Add(new Country { Id = _countryId, IsoCode = "US", Name = "United States" });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new CountryExistsByIsoQueryHandler(_dbContext);
        var result = await handler.Handle(new CountryExistsByIsoQuery("us"), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact(DisplayName = "CountryExists: Should return false when ISO code missing")]
    public async Task CountryExists_ShouldReturnFalse_WhenIsoCodeMissing()
    {
        var handler = new CountryExistsByIsoQueryHandler(_dbContext);
        var result = await handler.Handle(new CountryExistsByIsoQuery("XX"), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact(DisplayName = "StateExists: Should return true when state abbreviation exists under the country")]
    public async Task StateExists_ShouldReturnTrue_WhenStateExistsForCountry()
    {
        var country = new Country { Id = _countryId, IsoCode = "US", Name = "United States" };
        _dbContext.Set<Country>().Add(country);
        _dbContext.Set<State>().Add(new State
        {
            Id = Guid.NewGuid(),
            CountryId = _countryId,
            Country = country,
            Abbreviation = "CA",
            Name = "California"
        });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new StateExistsByIsoQueryHandler(_dbContext);
        var result = await handler.Handle(
            new StateExistsByIsoQuery("us", "ca"),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact(DisplayName = "StateExists: Should return false when state belongs to a different country")]
    public async Task StateExists_ShouldReturnFalse_WhenStateBelongsToAnotherCountry()
    {
        var country = new Country { Id = _countryId, IsoCode = "US", Name = "United States" };
        _dbContext.Set<Country>().Add(country);
        _dbContext.Set<State>().Add(new State
        {
            Id = Guid.NewGuid(),
            CountryId = _countryId,
            Country = country,
            Abbreviation = "CA",
            Name = "California"
        });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new StateExistsByIsoQueryHandler(_dbContext);
        var result = await handler.Handle(
            new StateExistsByIsoQuery("VN", "CA"),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }
}
