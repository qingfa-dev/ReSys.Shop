using Module.Location.Domain.Countries;
using Module.Location.Domain.States;
using Module.Location.Features.Admin.States.GetPagedOrAll;

using Shared.Operational.Persistence.Data;

namespace Module.UnitTests.Location.Features.Admin.States.Get.PagedOrAll;

[Trait("Category", "Unit")]
[Trait("Module", "Location")]
[Trait("Feature", "StatePagedOrAll")]
public class GetStatePagedOrAllUseCaseTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetStatePagedOrAll.PagedQueryHandler _handler;

    public GetStatePagedOrAllUseCaseTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Country).Assembly, typeof(State).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new GetStatePagedOrAll.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<Country> SetupCountryAsync(CancellationToken cancellationToken)
    {
        var country = new Country { Name = "USA", IsoCode = "US" };
        _dbContext.Set<Country>().Add(country);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await _dbContext.Set<Country>().FirstAsync(cancellationToken);
    }

    private async Task SetupStatesAsync(State[] states, CancellationToken cancellationToken)
    {
        var country = await SetupCountryAsync(cancellationToken);
        foreach (var state in states)
        {
            state.CountryId = country.Id;
        }
        _dbContext.Set<State>().AddRange(states);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    #region Basic Retrieval Tests

    [Fact(DisplayName = "Should return paged results when query is valid")]
    public async Task Handle_ShouldReturnPagedResults_WhenQueryIsValid()
    {
        var states = new[]
        {
            new State { Name = "California", Abbreviation = "CA", IsActive = true },
            new State { Name = "Texas", Abbreviation = "TX", IsActive = true },
            new State { Name = "New York", Abbreviation = "NY", IsActive = true }
        };

        await SetupStatesAsync(states, TestContext.Current.CancellationToken);

        var parameters = new GetStatePagedOrAll.Parameters { PageNumber = 1, PageSize = 10 };

        var result = await _handler.Handle(
            new GetStatePagedOrAll.Query(parameters),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact(DisplayName = "Should return empty result when no states exist")]
    public async Task Handle_ShouldReturnEmptyResult_WhenNoStatesExist()
    {
        var parameters = new GetStatePagedOrAll.Parameters { PageNumber = 1, PageSize = 10 };

        var result = await _handler.Handle(
            new GetStatePagedOrAll.Query(parameters),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    #endregion

    #region Pagination Tests

    [Theory]
    [InlineData(1, 10, 3)]
    [InlineData(1, 2, 2)]
    [InlineData(1, 1, 1)]
    public async Task Handle_ShouldRespectPageSizeParameter(int page, int pageSize, int expectedItems)
    {
        var states = new[]
        {
            new State { Name = "California", Abbreviation = "CA", IsActive = true },
            new State { Name = "Texas", Abbreviation = "TX", IsActive = true },
            new State { Name = "New York", Abbreviation = "NY", IsActive = true }
        };

        await SetupStatesAsync(states, TestContext.Current.CancellationToken);

        var parameters = new GetStatePagedOrAll.Parameters { PageNumber = page, PageSize = pageSize };

        var result = await _handler.Handle(
            new GetStatePagedOrAll.Query(parameters),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(expectedItems);
        result.TotalCount.Should().Be(3);
    }

    [Theory]
    [InlineData(2, 1, "B")]
    [InlineData(3, 1, "C")]
    public async Task Handle_ShouldReturnCorrectPage(int page, int pageSize, string expectedName)
    {
        var states = new[]
        {
            new State { Name = "A", Abbreviation = "AA", IsActive = true },
            new State { Name = "B", Abbreviation = "BB", IsActive = true },
            new State { Name = "C", Abbreviation = "CC", IsActive = true }
        };

        await SetupStatesAsync(states, TestContext.Current.CancellationToken);

        var parameters = new GetStatePagedOrAll.Parameters { PageNumber = page, PageSize = pageSize };

        var result = await _handler.Handle(
            new GetStatePagedOrAll.Query(parameters),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.PageNumber.Should().Be(page);
        result.Items.Should().HaveCount(1);
        result.Items.ElementAt(0).Name.Should().Be(expectedName);
    }

    #endregion

    #region Sort Tests

    public static TheoryData<string[], (string First, string Second, string Third)> SortNameTestData => new()
    {
        { ["Name"], ("Alpha", "Mexico", "Zebra") },
        { ["Name:asc"], ("Alpha", "Mexico", "Zebra") },
        { ["Name:desc"], ("Zebra", "Mexico", "Alpha") },
    };

    [Theory]
    [MemberData(nameof(SortNameTestData))]
    public async Task Handle_ShouldSortByNameCorrectly(string[] orderBy, (string First, string Second, string Third) expected)
    {
        var states = new[]
        {
            new State { Name = "Zebra", Abbreviation = "ZB", IsActive = true },
            new State { Name = "Alpha", Abbreviation = "AB", IsActive = true },
            new State { Name = "Mexico", Abbreviation = "MB", IsActive = true }
        };

        await SetupStatesAsync(states, TestContext.Current.CancellationToken);

        var parameters = new GetStatePagedOrAll.Parameters
        {
            PageNumber = 1,
            PageSize = 10,
            Sort = orderBy
        };

        var result = await _handler.Handle(
            new GetStatePagedOrAll.Query(parameters),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.ElementAt(0).Name.Should().Be(expected.First);
        result.Items.ElementAt(1).Name.Should().Be(expected.Second);
        result.Items.ElementAt(2).Name.Should().Be(expected.Third);
    }

    #endregion

    #region Search Tests

    [Theory]
    [InlineData("Cal", 1)]
    [InlineData("Cali", 1)]
    [InlineData("XYZ", 0)]
    public async Task Handle_ShouldSearchByName(string search, int expectedCount)
    {
        var states = new[]
        {
            new State { Name = "California", Abbreviation = "CA", IsActive = true },
            new State { Name = "Texas", Abbreviation = "TX", IsActive = true },
            new State { Name = "New York", Abbreviation = "NY", IsActive = true }
        };

        await SetupStatesAsync(states, TestContext.Current.CancellationToken);

        var parameters = new GetStatePagedOrAll.Parameters
        {
            PageNumber = 1,
            PageSize = 10,
            Search = search,
            SearchFields = [nameof(State.Name)]
        };

        var result = await _handler.Handle(
            new GetStatePagedOrAll.Query(parameters),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(expectedCount);
    }

    #endregion

    #region Filter Tests

    [Theory]
    [InlineData("Name eq 'California'")]
    [InlineData("Abbreviation eq 'CA'")]
    [InlineData("Name contains 'Cali'")]
    [InlineData("Abbreviation contains 'C'")]
    [InlineData("IsActive eq true")]
    public async Task Handle_ShouldAcceptVariousFilterFormats(string filter)
    {
        var states = new[]
        {
            new State { Name = "California", Abbreviation = "CA", IsActive = true },
            new State { Name = "Texas", Abbreviation = "TX", IsActive = true }
        };

        await SetupStatesAsync(states, TestContext.Current.CancellationToken);

        var parameters = new GetStatePagedOrAll.Parameters
        {
            PageNumber = 1,
            PageSize = 10,
            Filter = filter
        };

        var result = await _handler.Handle(
            new GetStatePagedOrAll.Query(parameters),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnAll_WhenFilterIsNull()
    {
        var states = new[]
        {
            new State { Name = "California", Abbreviation = "CA", IsActive = true },
            new State { Name = "Texas", Abbreviation = "TX", IsActive = true }
        };

        await SetupStatesAsync(states, TestContext.Current.CancellationToken);

        var parameters = new GetStatePagedOrAll.Parameters
        {
            PageNumber = 1,
            PageSize = 10,
            Filter = null
        };

        var result = await _handler.Handle(
            new GetStatePagedOrAll.Query(parameters),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnAll_WhenFilterIsEmpty()
    {
        var states = new[]
        {
            new State { Name = "California", Abbreviation = "CA", IsActive = true },
            new State { Name = "Texas", Abbreviation = "TX", IsActive = true }
        };

        await SetupStatesAsync(states, TestContext.Current.CancellationToken);

        var parameters = new GetStatePagedOrAll.Parameters
        {
            PageNumber = 1,
            PageSize = 10,
            Filter = ""
        };

        var result = await _handler.Handle(
            new GetStatePagedOrAll.Query(parameters),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
    }

    #endregion

    #region Mapping Tests

    [Fact(DisplayName = "Should map to list item response")]
    public async Task Handle_ShouldMapToListItemResponse()
    {
        var state = new State
        {
            Name = "California",
            Abbreviation = "CA",
            IsActive = true
        };

        await SetupStatesAsync([state], TestContext.Current.CancellationToken);

        var parameters = new GetStatePagedOrAll.Parameters { PageNumber = 1, PageSize = 10 };

        var result = await _handler.Handle(
            new GetStatePagedOrAll.Query(parameters),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.ElementAt(0).Name.Should().Be("California");
        result.Items.ElementAt(0).Abbreviation.Should().Be("CA");
    }

    [Fact(DisplayName = "Should map IsActive property")]
    public async Task Handle_ShouldMapIsActiveProperty()
    {
        var state = new State
        {
            Name = "Active State",
            Abbreviation = "AS",
            IsActive = true
        };

        await SetupStatesAsync([state], TestContext.Current.CancellationToken);

        var parameters = new GetStatePagedOrAll.Parameters { PageNumber = 1, PageSize = 10 };

        var result = await _handler.Handle(
            new GetStatePagedOrAll.Query(parameters),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.ElementAt(0).IsActive.Should().BeTrue();
    }

    #endregion
}