using Module.Location.Domain.Countries;
using Module.Location.Domain.States;
using Module.Location.Features.Admin.States.Update;

using Shared.Operational.Persistence.Data;

namespace Module.UnitTests.Location.Features.Admin.States.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Location")]
[Trait("Feature", "StateUpdate")]
public class UpdateStateTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<UpdateState.CommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly UpdateState.CommandHandler _handler;

    public UpdateStateTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(State).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<UpdateState.CommandHandler>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _handler = new UpdateState.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Should update state successfully")]
    public async Task Handle_ShouldReturnSuccess_WhenStateExists()
    {
        var country = new Country { Name = "United States", IsoCode = "US" };
        var state = new State { Name = "California", Abbreviation = "CA", CountryId = country.Id };
        _dbContext.Set<Country>().Add(country);
        _dbContext.Set<State>().Add(state);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateState.Request
        {
            Name = "California State", Abbreviation = "CA", CountryId = country.Id
        };

        var result = await _handler.Handle(
            new UpdateState.Command(state.Id, request),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("California State");
        result.Value.Abbreviation.Should().Be("CA");
    }

    [Fact(DisplayName = "Should return NotFound when state doesn't exist")]
    public async Task Handle_ShouldReturnFailure_WhenStateNotFound()
    {
        var nonExistentId = Guid.NewGuid();
        var country = new Country { Name = "United States", IsoCode = "US" };
        _dbContext.Set<Country>().Add(country);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateState.Request { Name = "Test", Abbreviation = "TT", CountryId = country.Id };

        var result = await _handler.Handle(
            new UpdateState.Command(nonExistentId, request),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(StateResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Should return CountryNotFound when country doesn't exist")]
    public async Task Handle_ShouldReturnFailure_WhenCountryNotFound()
    {
        var country = new Country { Name = "United States", IsoCode = "US" };
        var state = new State { Name = "California", Abbreviation = "CA", CountryId = country.Id };
        _dbContext.Set<Country>().Add(country);
        _dbContext.Set<State>().Add(state);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateState.Request { Name = "California", Abbreviation = "CA", CountryId = Guid.NewGuid() };

        var result = await _handler.Handle(
            new UpdateState.Command(state.Id, request),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(StateResult.Failure.CountryNotFound.Code);
    }

    [Fact(DisplayName = "Should return duplicate error when Abbreviation belongs to another state in same country")]
    public async Task Handle_ShouldReturnFailure_WhenAbbreviationIsDuplicate()
    {
        var country = new Country { Name = "United States", IsoCode = "US" };
        var state1 = new State { Name = "California", Abbreviation = "CA", CountryId = country.Id };
        var state2 = new State { Name = "Colorado", Abbreviation = "CO", CountryId = country.Id };
        _dbContext.Set<Country>().Add(country);
        _dbContext.Set<State>().AddRange(state1, state2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateState.Request { Name = "Trying Colorado", Abbreviation = "CO", CountryId = country.Id };

        var result = await _handler.Handle(
            new UpdateState.Command(state1.Id, request),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(StateResult.Failure.AbbreviationDuplicate.Code);
    }

    [Fact(DisplayName = "Should allow same Abbreviation (current state unchanged)")]
    public async Task Handle_ShouldAllowSameAbbreviation()
    {
        var country = new Country { Name = "United States", IsoCode = "US" };
        var state = new State { Name = "California", Abbreviation = "CA", CountryId = country.Id };
        _dbContext.Set<Country>().Add(country);
        _dbContext.Set<State>().Add(state);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateState.Request
        {
            Name = "California State", Abbreviation = "CA", CountryId = country.Id
        };

        var result = await _handler.Handle(
            new UpdateState.Command(state.Id, request),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Abbreviation.Should().Be("CA");
    }

    [Fact(DisplayName = "Should set ModifiedAt timestamp")]
    public async Task Handle_ShouldSetModifiedTimestamp()
    {
        var country = new Country { Name = "United States", IsoCode = "US" };
        var state = new State { Name = "California", Abbreviation = "CA", CountryId = country.Id };
        _dbContext.Set<Country>().Add(country);
        _dbContext.Set<State>().Add(state);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateState.Request
        {
            Name = "California State", Abbreviation = "CA", CountryId = country.Id
        };

        var result = await _handler.Handle(
            new UpdateState.Command(state.Id, request),
            TestContext.Current.CancellationToken);

        result.Value.ModifiedAtUtc.Should().NotBeNull();
        result.Value.ModifiedAtUtc.Should().NotBe(default);
    }

    [Fact(DisplayName = "Should update all properties")]
    public async Task Handle_ShouldUpdateAllProperties()
    {
        var country = new Country { Name = "United States", IsoCode = "US" };
        var state = new State { Name = "Old Name", Abbreviation = "ON", CountryId = country.Id, IsActive = true };
        _dbContext.Set<Country>().Add(country);
        _dbContext.Set<State>().Add(state);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateState.Request
        {
            Name = "New Name", Abbreviation = "NN", CountryId = country.Id, IsActive = false
        };

        var result = await _handler.Handle(
            new UpdateState.Command(state.Id, request),
            TestContext.Current.CancellationToken);

        result.Value.Name.Should().Be("New Name");
        result.Value.Abbreviation.Should().Be("NN");
        result.Value.IsActive.Should().BeFalse();
    }

    [Fact(DisplayName = "Should persist changes to database")]
    public async Task Handle_ShouldPersistChanges_ToDatabase()
    {
        var country = new Country { Name = "United States", IsoCode = "US" };
        var state = new State { Name = "California", Abbreviation = "CA", CountryId = country.Id };
        _dbContext.Set<Country>().Add(country);
        _dbContext.Set<State>().Add(state);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var stateId = state.Id;
        var request = new UpdateState.Request
        {
            Name = "California Updated", Abbreviation = "CA", CountryId = country.Id
        };

        await _handler.Handle(
            new UpdateState.Command(stateId, request),
            TestContext.Current.CancellationToken);

        var updatedState = await _dbContext.Set<State>()
            .FirstOrDefaultAsync(s => s.Id == stateId, TestContext.Current.CancellationToken);

        updatedState.Should().NotBeNull();
        updatedState.Name.Should().Be("California Updated");
        updatedState.ModifiedAtUtc.Should().NotBeNull();
    }

    [Fact(DisplayName = "Should allow changing country to different country")]
    public async Task Handle_ShouldAllowChangingCountry()
    {
        var country1 = new Country { Name = "United States", IsoCode = "US" };
        var country2 = new Country { Name = "Canada", IsoCode = "CA" };
        var state = new State { Name = "California", Abbreviation = "CA", CountryId = country1.Id };
        _dbContext.Set<Country>().AddRange(country1, country2);
        _dbContext.Set<State>().Add(state);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateState.Request { Name = "California", Abbreviation = "CA", CountryId = country2.Id };

        var result = await _handler.Handle(
            new UpdateState.Command(state.Id, request),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.CountryId.Should().Be(country2.Id);
    }

    [Fact(DisplayName = "Should allow abbreviation when same abbreviation exists in different country")]
    public async Task Handle_ShouldAllowAbbreviation_WhenExistsInDifferentCountry()
    {
        var country1 = new Country { Name = "United States", IsoCode = "US" };
        var country2 = new Country { Name = "Canada", IsoCode = "CA" };
        var state1 = new State { Name = "California", Abbreviation = "CA", CountryId = country1.Id };
        var state2 = new State { Name = "Ontario", Abbreviation = "ON", CountryId = country2.Id };
        _dbContext.Set<Country>().AddRange(country1, country2);
        _dbContext.Set<State>().AddRange(state1, state2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateState.Request { Name = "California", Abbreviation = "ON", CountryId = country1.Id };

        var result = await _handler.Handle(
            new UpdateState.Command(state1.Id, request),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }
}