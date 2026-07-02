using Microsoft.EntityFrameworkCore;

using Module.Location.Domain.Countries;
using Module.Location.Domain.States;
using Module.Location.Features.Admin.States.GetById;

using Shared.Operational.Persistence.Data;

namespace Module.UnitTests.Location.Features.Admin.States.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Locations")]
[Trait("Feature", "StateGetById")]
public class GetStateByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetStateById.QueryHandler _handler;

    public GetStateByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(State).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new GetStateById.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Should return state when Id exists")]
    public async Task Handle_ShouldReturnState_WhenIdExists()
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
            new GetStateById.Query(state.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Abbreviation.Should().Be("CA");
    }

    [Fact(DisplayName = "Should return NotFound when Id doesn't exist")]
    public async Task Handle_ShouldReturnNotFound_WhenIdNotFound()
    {
        var nonExistentId = Guid.NewGuid();

        var result = await _handler.Handle(
            new GetStateById.Query(nonExistentId),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(StateResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "Should return correct state from multiple")]
    public async Task Handle_ShouldReturnCorrectState_FromMultiple()
    {
        var country = new Country { Name = "United States", IsoCode = "US" };
        var state1 = new State { Name = "California", Abbreviation = "CA", CountryId = country.Id };
        var state2 = new State { Name = "Texas", Abbreviation = "TX", CountryId = country.Id };
        var state3 = new State { Name = "New York", Abbreviation = "NY", CountryId = country.Id };
        _dbContext.Set<Country>().Add(country);
        _dbContext.Set<State>().AddRange(state1, state2, state3);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetStateById.Query(state2.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Texas");
    }

    [Fact(DisplayName = "Should return state with CountryId")]
    public async Task Handle_ShouldReturnState_WithCountryId()
    {
        var country = new Country { Name = "United States", IsoCode = "US" };
        var state = new State
        {
            Name = "California",
            Abbreviation = "CA",
            CountryId = country.Id,
            IsActive = true
        };
        _dbContext.Set<Country>().Add(country);
        _dbContext.Set<State>().Add(state);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetStateById.Query(state.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.CountryId.Should().Be(country.Id);
        result.Value.IsActive.Should().BeTrue();
    }
}