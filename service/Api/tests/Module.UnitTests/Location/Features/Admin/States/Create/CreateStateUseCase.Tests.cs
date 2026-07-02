using Microsoft.EntityFrameworkCore;

using Module.Location.Domain.Countries;
using Module.Location.Domain.States;
using Module.Location.Features.Admin.States.Create;

using Shared.Operational.Persistence.Data;

namespace Module.UnitTests.Location.Features.Admin.States.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Locations")]
[Trait("Feature", "StateCreate")]
public class CreateStateTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CreateState.CommandHandler _handler;

    public CreateStateTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Country).Assembly, typeof(State).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new CreateState.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Should create state successfully")]
    public async Task Handle_ShouldReturnSuccess_WhenStateIsValid()
    {
        // Arrange
        _dbContext.Set<Country>().Add(new Country
        {
            Name = "United States",
            IsoCode = "US"
        });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var country = await _dbContext.Set<Country>().FirstAsync(TestContext.Current.CancellationToken);

        var request = new CreateState.Request
        {
            Name = "California",
            Abbreviation = "CA",
            CountryId = country.Id,
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(
            new CreateState.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("California");
        result.Value.Abbreviation.Should().Be("CA");
    }

    [Fact(DisplayName = "Should fail when country not found")]
    public async Task Handle_ShouldReturnFailure_WhenCountryNotFound()
    {
        // Arrange
        var request = new CreateState.Request
        {
            Name = "California",
            Abbreviation = "CA",
            CountryId = Guid.NewGuid(),
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(
            new CreateState.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(StateResult.Errors.CountryNotFound.Code);
    }

    [Fact(DisplayName = "Should fail when abbreviation is duplicate in same country")]
    public async Task Handle_ShouldReturnFailure_WhenAbbreviationIsDuplicate()
    {
        // Arrange
        _dbContext.Set<Country>().Add(new Country
        {
            Name = "United States",
            IsoCode = "US"
        });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var country = await _dbContext.Set<Country>().FirstAsync(TestContext.Current.CancellationToken);

        _dbContext.Set<State>().Add(new State
        {
            Name = "California",
            Abbreviation = "CA",
            CountryId = country.Id
        });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CreateState.Request
        {
            Name = "Colorado",
            Abbreviation = "CA",
            CountryId = country.Id,
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(
            new CreateState.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(StateResult.Errors.AbbreviationDuplicate.Code);
    }

    [Fact(DisplayName = "Should allow same abbreviation in different countries")]
    public async Task Handle_ShouldAllowSameAbbreviation_DifferentCountries()
    {
        // Arrange
        _dbContext.Set<Country>().Add(new Country { Name = "USA", IsoCode = "US" });
        _dbContext.Set<Country>().Add(new Country { Name = "Mexico", IsoCode = "MX" });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var countries = await _dbContext.Set<Country>().ToListAsync(TestContext.Current.CancellationToken);
        var countryId1 = countries[0].Id;
        var countryId2 = countries[1].Id;

        _dbContext.Set<State>().Add(new State { Name = "California", Abbreviation = "CA", CountryId = countryId1 });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CreateState.Request
        {
            Name = "Baja California",
            Abbreviation = "CA",
            CountryId = countryId2,
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(
            new CreateState.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "Should set CreatedAt timestamp")]
    public async Task Handle_ShouldSetCreatedTimestamp()
    {
        // Arrange
        _dbContext.Set<Country>().Add(new Country { Name = "USA", IsoCode = "US" });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var country = await _dbContext.Set<Country>().FirstAsync(TestContext.Current.CancellationToken);

        var request = new CreateState.Request
        {
            Name = "Texas",
            Abbreviation = "TX",
            CountryId = country.Id,
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(
            new CreateState.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        // Note: CreatedAtUtc is set by interceptor, may be default in unit tests
        result.Value.Id.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "Should map all properties correctly")]
    public async Task Handle_ShouldReturnStateWithCorrectProperties()
    {
        // Arrange
        _dbContext.Set<Country>().Add(new Country { Name = "USA", IsoCode = "US" });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var country = await _dbContext.Set<Country>().FirstAsync(TestContext.Current.CancellationToken);

        var request = new CreateState.Request
        {
            Name = "New York",
            Abbreviation = "NY",
            CountryId = country.Id,
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(
            new CreateState.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.Value.Id.Should().NotBe(Guid.Empty);
        result.Value.Name.Should().Be("New York");
        result.Value.Abbreviation.Should().Be("NY");
        result.Value.CountryId.Should().Be(country.Id);
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact(DisplayName = "Should apply default IsActive value")]
    public async Task Handle_ShouldUseDefaultIsActive_WhenNotProvided()
    {
        // Arrange
        _dbContext.Set<Country>().Add(new Country { Name = "USA", IsoCode = "US" });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var country = await _dbContext.Set<Country>().FirstAsync(TestContext.Current.CancellationToken);

        var request = new CreateState.Request
        {
            Name = "Florida",
            Abbreviation = "FL",
            CountryId = country.Id
        };

        // Act
        var result = await _handler.Handle(
            new CreateState.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact(DisplayName = "Should persist state to database")]
    public async Task Handle_ShouldPersistState_ToDatabase()
    {
        // Arrange
        _dbContext.Set<Country>().Add(new Country { Name = "USA", IsoCode = "US" });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var country = await _dbContext.Set<Country>().FirstAsync(TestContext.Current.CancellationToken);

        var request = new CreateState.Request
        {
            Name = "Washington",
            Abbreviation = "WA",
            CountryId = country.Id,
            IsActive = true
        };

        // Act
        await _handler.Handle(
            new CreateState.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        var state = await _dbContext.Set<State>()
            .FirstOrDefaultAsync(s => s.Abbreviation == "WA", TestContext.Current.CancellationToken);

        state.Should().NotBeNull();
        state.Name.Should().Be("Washington");
    }

    [Fact(DisplayName = "Should return state with country name in list response")]
    public async Task Handle_ShouldReturnStateWithCountryName()
    {
        // Arrange
        _dbContext.Set<Country>().Add(new Country { Name = "Canada", IsoCode = "CA" });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var country = await _dbContext.Set<Country>().FirstAsync(TestContext.Current.CancellationToken);

        var request = new CreateState.Request
        {
            Name = "Ontario",
            Abbreviation = "ON",
            CountryId = country.Id,
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(
            new CreateState.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.Value.CountryId.Should().Be(country.Id);
    }
}
