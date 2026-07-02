using Module.Location.Domain.Countries;
using Module.Location.Domain.States;
using Module.Location.Features.Admin.Countries.Delete;

using Shared.Operational.Persistence.Data;

namespace Module.UnitTests.Location.Features.Admin.Countries.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Location")]
[Trait("Feature", "CountryDelete")]
public class DeleteCountryTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DeleteCountry.CommandHandler _handler;

    public DeleteCountryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Country).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new DeleteCountry.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Should delete country when it exists and has no states")]
    public async Task Handle_ShouldDeleteCountry_WhenExistsAndHasNoStates()
    {
        // Arrange
        var country = new Country
        {
            Name = "United States",
            IsoCode = "US",
            CallingCode = "+1",
            StatesRequired = true,
            IsActive = true
        };
        _dbContext.Set<Country>().Add(country);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new DeleteCountry.Command(country.Id),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(country.Id);
        result.Value.Name.Should().Be("United States");
    }

    [Fact(DisplayName = "Should return NotFound when country doesn't exist")]
    public async Task Handle_ShouldReturnNotFound_WhenCountryNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _handler.Handle(
            new DeleteCountry.Command(nonExistentId),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(CountryResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Should return error when country has states")]
    public async Task Handle_ShouldReturnError_WhenCountryHasStates()
    {
        // Arrange
        var country = new Country
        {
            Name = "United States",
            IsoCode = "US",
            CallingCode = "+1",
            StatesRequired = true,
            IsActive = true
        };
        var state = new State { Name = "California", Abbreviation = "CA", CountryId = country.Id };
        _dbContext.Set<Country>().Add(country);
        _dbContext.Set<State>().Add(state);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new DeleteCountry.Command(country.Id),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(CountryResult.Failure.CannotDeleteWithStates.Code);
    }

    [Fact(DisplayName = "Should return error when country has multiple states")]
    public async Task Handle_ShouldReturnError_WhenCountryHasMultipleStates()
    {
        // Arrange
        var country = new Country
        {
            Name = "United States",
            IsoCode = "US",
            CallingCode = "+1",
            StatesRequired = true,
            IsActive = true
        };
        var state1 = new State { Name = "California", Abbreviation = "CA", CountryId = country.Id };
        var state2 = new State { Name = "Texas", Abbreviation = "TX", CountryId = country.Id };
        var state3 = new State { Name = "New York", Abbreviation = "NY", CountryId = country.Id };
        _dbContext.Set<Country>().Add(country);
        _dbContext.Set<State>().AddRange(state1, state2, state3);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new DeleteCountry.Command(country.Id),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(CountryResult.Failure.CannotDeleteWithStates.Code);
    }

    [Fact(DisplayName = "Should actually remove country from database")]
    public async Task Handle_ShouldRemoveCountryFromDatabase()
    {
        // Arrange
        var country = new Country
        {
            Name = "Canada",
            IsoCode = "CA",
            CallingCode = "+1",
            StatesRequired = true,
            IsActive = true
        };
        _dbContext.Set<Country>().Add(country);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new DeleteCountry.Command(country.Id),
            TestContext.Current.CancellationToken);

        // Assert
        var deletedCountry = await _dbContext.Set<Country>()
            .FirstOrDefaultAsync(c => c.Id == country.Id, TestContext.Current.CancellationToken);
        deletedCountry.Should().BeNull();
    }

    [Fact(DisplayName = "Should delete correct country from multiple")]
    public async Task Handle_ShouldDeleteCorrectCountry_FromMultiple()
    {
        // Arrange
        var country1 = new Country { Name = "United States", IsoCode = "US", CallingCode = "+1" };
        var country2 = new Country { Name = "Canada", IsoCode = "CA", CallingCode = "+1" };
        var country3 = new Country { Name = "Mexico", IsoCode = "MX", CallingCode = "+52" };
        _dbContext.Set<Country>().AddRange(country1, country2, country3);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new DeleteCountry.Command(country2.Id),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Canada");

        var remainingCountries = await _dbContext.Set<Country>().ToListAsync(TestContext.Current.CancellationToken);
        remainingCountries.Should().HaveCount(2);
        remainingCountries.Should().Contain(c => c.IsoCode == "US");
        remainingCountries.Should().NotContain(c => c.IsoCode == "CA");
    }

    [Fact(DisplayName = "Should preserve other countries when deleting one")]
    public async Task Handle_ShouldPreserveOtherCountries_WhenDeletingOne()
    {
        // Arrange
        var country1 = new Country { Name = "United States", IsoCode = "US", CallingCode = "+1" };
        var country2 = new Country { Name = "Canada", IsoCode = "CA", CallingCode = "+1" };
        _dbContext.Set<Country>().AddRange(country1, country2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new DeleteCountry.Command(country1.Id),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var remainingCountries = await _dbContext.Set<Country>().ToListAsync(TestContext.Current.CancellationToken);
        remainingCountries.Should().HaveCount(1);
        remainingCountries[0].IsoCode.Should().Be("CA");
    }
}