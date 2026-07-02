using Microsoft.EntityFrameworkCore;

using Module.Location.Domain.Countries;
using Module.Location.Domain.States;
using Module.Location.Features.Admin.States.Delete;

using Shared.Operational.Persistence.Data;

namespace Module.UnitTests.Location.Features.Admin.States.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Locations")]
[Trait("Feature", "StateDelete")]
public class DeleteStateTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DeleteState.CommandHandler _handler;

    public DeleteStateTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(State).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new DeleteState.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Should delete state when it exists")]
    public async Task Handle_ShouldDeleteState_WhenExists()
    {
        var country = new Country { Name = "United States", IsoCode = "US" };
        var state = new State
        {
            Name = "California",
            Abbreviation = "CA",
            CountryId = country.Id
        };
        _dbContext.Set<Country>().Add(country);
        _dbContext.Set<State>().Add(state);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new DeleteState.Command(state.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(state.Id);
        result.Value.Name.Should().Be("California");
    }

    [Fact(DisplayName = "Should return NotFound when state doesn't exist")]
    public async Task Handle_ShouldReturnNotFound_WhenStateNotFound()
    {
        var nonExistentId = Guid.NewGuid();

        var result = await _handler.Handle(
            new DeleteState.Command(nonExistentId),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(StateResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "Should actually remove state from database")]
    public async Task Handle_ShouldRemoveStateFromDatabase()
    {
        var country = new Country { Name = "United States", IsoCode = "US" };
        var state = new State
        {
            Name = "California",
            Abbreviation = "CA",
            CountryId = country.Id
        };
        _dbContext.Set<Country>().Add(country);
        _dbContext.Set<State>().Add(state);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new DeleteState.Command(state.Id),
            TestContext.Current.CancellationToken);

        var deletedState = await _dbContext.Set<State>()
            .FirstOrDefaultAsync(s => s.Id == state.Id, TestContext.Current.CancellationToken);
        deletedState.Should().BeNull();
    }

    [Fact(DisplayName = "Should delete correct state from multiple")]
    public async Task Handle_ShouldDeleteCorrectState_FromMultiple()
    {
        var country = new Country { Name = "United States", IsoCode = "US" };
        var state1 = new State { Name = "California", Abbreviation = "CA", CountryId = country.Id };
        var state2 = new State { Name = "Texas", Abbreviation = "TX", CountryId = country.Id };
        var state3 = new State { Name = "New York", Abbreviation = "NY", CountryId = country.Id };
        _dbContext.Set<Country>().Add(country);
        _dbContext.Set<State>().AddRange(state1, state2, state3);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new DeleteState.Command(state2.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Texas");

        var remainingStates = await _dbContext.Set<State>().ToListAsync(TestContext.Current.CancellationToken);
        remainingStates.Should().HaveCount(2);
        remainingStates.Should().Contain(s => s.Abbreviation == "CA");
        remainingStates.Should().Contain(s => s.Abbreviation == "NY");
        remainingStates.Should().NotContain(s => s.Abbreviation == "TX");
    }

    [Fact(DisplayName = "Should preserve other states when deleting one")]
    public async Task Handle_ShouldPreserveOtherStates_WhenDeletingOne()
    {
        var country = new Country { Name = "United States", IsoCode = "US" };
        var state1 = new State { Name = "California", Abbreviation = "CA", CountryId = country.Id };
        var state2 = new State { Name = "Texas", Abbreviation = "TX", CountryId = country.Id };
        _dbContext.Set<Country>().Add(country);
        _dbContext.Set<State>().AddRange(state1, state2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new DeleteState.Command(state1.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var remainingStates = await _dbContext.Set<State>().ToListAsync(TestContext.Current.CancellationToken);
        remainingStates.Should().HaveCount(1);
        remainingStates[0].Abbreviation.Should().Be("TX");
    }

    [Fact(DisplayName = "Should preserve states in other countries")]
    public async Task Handle_ShouldPreserveStatesInOtherCountries()
    {
        var country1 = new Country { Name = "United States", IsoCode = "US" };
        var country2 = new Country { Name = "Canada", IsoCode = "CA" };
        var state1 = new State { Name = "California", Abbreviation = "CA", CountryId = country1.Id };
        var state2 = new State { Name = "Ontario", Abbreviation = "ON", CountryId = country2.Id };
        _dbContext.Set<Country>().AddRange(country1, country2);
        _dbContext.Set<State>().AddRange(state1, state2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new DeleteState.Command(state1.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var remainingStates = await _dbContext.Set<State>().ToListAsync(TestContext.Current.CancellationToken);
        remainingStates.Should().HaveCount(1);
        remainingStates[0].CountryId.Should().Be(country2.Id);
    }

    [Fact(DisplayName = "Should return state with correct Id and Name in response")]
    public async Task Handle_ShouldReturnCorrectIdAndName()
    {
        var country = new Country { Name = "United States", IsoCode = "US" };
        var state = new State
        {
            Name = "California",
            Abbreviation = "CA",
            CountryId = country.Id
        };
        _dbContext.Set<Country>().Add(country);
        _dbContext.Set<State>().Add(state);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new DeleteState.Command(state.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(state.Id);
        result.Value.Name.Should().Be("California");
    }
}