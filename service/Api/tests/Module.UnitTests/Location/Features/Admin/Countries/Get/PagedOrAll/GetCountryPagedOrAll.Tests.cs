using Module.Location.Domain.Countries;
using Module.Location.Features.Admin.Countries.GetPagedOrAll;

using Shared.Operational.Persistence.Data;

namespace Module.UnitTests.Location.Features.Admin.Countries.Get.PagedOrAll;

[Trait("Category", "Unit")]
[Trait("Module", "Location")]
[Trait("Feature", "CountryPagedOrAll")]
public class GetCountryPagedOrAllTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetCountryPagedOrAll.PagedQueryHandler _handler;

    public GetCountryPagedOrAllTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Country).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new GetCountryPagedOrAll.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task SetupCountriesAsync(Country[] countries, CancellationToken cancellationToken)
    {
        _dbContext.Set<Country>().AddRange(countries);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    #region Basic Retrieval Tests

    [Fact(DisplayName = "Should return paged results when query is valid")]
    public async Task Handle_ShouldReturnPagedResults_WhenQueryIsValid()
    {
        var countries = new[]
        {
            new Country { Name = "United States", IsoCode = "US", CallingCode = "+1", StatesRequired = true, IsActive = true },
            new Country { Name = "Canada", IsoCode = "CA", CallingCode = "+1", StatesRequired = true, IsActive = true },
            new Country { Name = "Mexico", IsoCode = "MX", CallingCode = "+52", StatesRequired = true, IsActive = true }
        };

        await SetupCountriesAsync(countries, TestContext.Current.CancellationToken);

        var parameters = new GetCountryPagedOrAll.Parameters { PageNumber = 1, PageSize = 10 };

        var result = await _handler.Handle(
            new GetCountryPagedOrAll.Query(parameters),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact(DisplayName = "Should return empty result when no countries exist")]
    public async Task Handle_ShouldReturnEmptyResult_WhenNoCountriesExist()
    {
        var parameters = new GetCountryPagedOrAll.Parameters { PageNumber = 1, PageSize = 10 };

        var result = await _handler.Handle(
            new GetCountryPagedOrAll.Query(parameters),
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
        var countries = new[]
        {
            new Country { Name = "United States", IsoCode = "US", CallingCode = "+1", StatesRequired = true, IsActive = true },
            new Country { Name = "Canada", IsoCode = "CA", CallingCode = "+1", StatesRequired = true, IsActive = true },
            new Country { Name = "Mexico", IsoCode = "MX", CallingCode = "+52", StatesRequired = true, IsActive = true }
        };

        await SetupCountriesAsync(countries, TestContext.Current.CancellationToken);

        var parameters = new GetCountryPagedOrAll.Parameters { PageNumber = page, PageSize = pageSize };

        var result = await _handler.Handle(
            new GetCountryPagedOrAll.Query(parameters),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(expectedItems);
        result.TotalCount.Should().Be(3);
    }

    [Theory]
    [InlineData(2, 1, "B")]
    [InlineData(3, 1, "C")]
    public async Task Handle_ShouldReturnCorrectPage(int pageNumber, int pageSize, string expectedName)
    {
        var countries = new[]
        {
            new Country { Name = "A", IsoCode = "A", CallingCode = "+1", StatesRequired = true, IsActive = true },
            new Country { Name = "B", IsoCode = "B", CallingCode = "+1", StatesRequired = true, IsActive = true },
            new Country { Name = "C", IsoCode = "C", CallingCode = "+1", StatesRequired = true, IsActive = true }
        };

        await SetupCountriesAsync(countries, TestContext.Current.CancellationToken);

        var parameters = new GetCountryPagedOrAll.Parameters { PageNumber = pageNumber, PageSize = pageSize };

        var result = await _handler.Handle(
            new GetCountryPagedOrAll.Query(parameters),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.PageNumber.Should().Be(pageNumber);
        result.Items.Should().HaveCount(1);
        result.Items.ElementAt(0).Name.Should().Be(expectedName);
    }

    #endregion

    #region Sort Tests

    public static TheoryData<string[], (string First, string Second, string Third)> SortNameTestData => new()
    {
        { ["Name"], ("Argentina", "Mexico", "Zimbabwe") },
        { ["Name:asc"], ("Argentina", "Mexico", "Zimbabwe") },
        { ["Name:desc"], ("Zimbabwe", "Mexico", "Argentina") },
    };

    [Theory]
    [MemberData(nameof(SortNameTestData))]
    public async Task Handle_ShouldSortByNameCorrectly(string[] orderBy, (string First, string Second, string Third) expected)
    {
        var countries = new[]
        {
            new Country { Name = "Zimbabwe", IsoCode = "ZW", CallingCode = "+263", StatesRequired = false, IsActive = true },
            new Country { Name = "Argentina", IsoCode = "AR", CallingCode = "+54", StatesRequired = true, IsActive = true },
            new Country { Name = "Mexico", IsoCode = "MX", CallingCode = "+52", StatesRequired = true, IsActive = true }
        };

        await SetupCountriesAsync(countries, TestContext.Current.CancellationToken);

        var parameters = new GetCountryPagedOrAll.Parameters
        {
            PageNumber = 1,
            PageSize = 10,
            Sort = orderBy
        };

        var result = await _handler.Handle(
            new GetCountryPagedOrAll.Query(parameters),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.ElementAt(0).Name.Should().Be(expected.First);
        result.Items.ElementAt(1).Name.Should().Be(expected.Second);
        result.Items.ElementAt(2).Name.Should().Be(expected.Third);
    }

    #endregion

    #region Search Tests

    [Theory]
    [InlineData("United", 1)]
    [InlineData("United States", 1)]
    [InlineData("XYZ", 0)]
    public async Task Handle_ShouldSearchByName(string search, int expectedCount)
    {
        var countries = new[]
        {
            new Country { Name = "United States", IsoCode = "US", CallingCode = "+1", StatesRequired = true, IsActive = true },
            new Country { Name = "Canada", IsoCode = "CA", CallingCode = "+1", StatesRequired = true, IsActive = true },
            new Country { Name = "Mexico", IsoCode = "MX", CallingCode = "+52", StatesRequired = true, IsActive = true }
        };

        await SetupCountriesAsync(countries, TestContext.Current.CancellationToken);

        var parameters = new GetCountryPagedOrAll.Parameters
        {
            PageNumber = 1,
            PageSize = 10,
            Search = search,
            SearchFields = [nameof(Country.Name)]
        };

        var result = await _handler.Handle(
            new GetCountryPagedOrAll.Query(parameters),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(expectedCount);
    }

    #endregion

    #region Filter Tests

    [Theory]
    [InlineData("Name eq 'United States'")]
    [InlineData("IsoCode eq 'US'")]
    [InlineData("Name contains 'United'")]
    [InlineData("IsoCode contains 'U'")]
    [InlineData("IsActive eq true")]
    public async Task Handle_ShouldAcceptVariousFilterFormats(string filter)
    {
        var countries = new[]
        {
            new Country { Name = "United States", IsoCode = "US", CallingCode = "+1", StatesRequired = true, IsActive = true },
            new Country { Name = "Canada", IsoCode = "CA", CallingCode = "+1", StatesRequired = true, IsActive = true }
        };

        await SetupCountriesAsync(countries, TestContext.Current.CancellationToken);

        var parameters = new GetCountryPagedOrAll.Parameters
        {
            PageNumber = 1,
            PageSize = 10,
            Filter = filter
        };

        var result = await _handler.Handle(
            new GetCountryPagedOrAll.Query(parameters),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnAll_WhenFilterIsNull()
    {
        var countries = new[]
        {
            new Country { Name = "United States", IsoCode = "US", CallingCode = "+1", StatesRequired = true, IsActive = true },
            new Country { Name = "Canada", IsoCode = "CA", CallingCode = "+1", StatesRequired = true, IsActive = true }
        };

        await SetupCountriesAsync(countries, TestContext.Current.CancellationToken);

        var parameters = new GetCountryPagedOrAll.Parameters
        {
            PageNumber = 1,
            PageSize = 10,
            Filter = null
        };

        var result = await _handler.Handle(
            new GetCountryPagedOrAll.Query(parameters),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnAll_WhenFilterIsEmpty()
    {
        var countries = new[]
        {
            new Country { Name = "United States", IsoCode = "US", CallingCode = "+1", StatesRequired = true, IsActive = true },
            new Country { Name = "Canada", IsoCode = "CA", CallingCode = "+1", StatesRequired = true, IsActive = true }
        };

        await SetupCountriesAsync(countries, TestContext.Current.CancellationToken);

        var parameters = new GetCountryPagedOrAll.Parameters
        {
            PageNumber = 1,
            PageSize = 10,
            Filter = ""
        };

        var result = await _handler.Handle(
            new GetCountryPagedOrAll.Query(parameters),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
    }

    #endregion

    #region Mapping Tests

    [Fact(DisplayName = "Should map to list item response")]
    public async Task Handle_ShouldMapToListItemResponse()
    {
        var country = new Country
        {
            Name = "United States",
            IsoCode = "US",
            CallingCode = "+1",
            StatesRequired = true,
            IsActive = true
        };

        await SetupCountriesAsync([country], TestContext.Current.CancellationToken);

        var parameters = new GetCountryPagedOrAll.Parameters { PageNumber = 1, PageSize = 10 };

        var result = await _handler.Handle(
            new GetCountryPagedOrAll.Query(parameters),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.ElementAt(0).Name.Should().Be("United States");
        result.Items.ElementAt(0).IsoCode.Should().Be("US");
        result.Items.ElementAt(0).CallingCode.Should().Be("+1");
    }

    [Fact(DisplayName = "Should map StatesRequired property")]
    public async Task Handle_ShouldMapStatesRequiredProperty()
    {
        var country = new Country
        {
            Name = "United States",
            IsoCode = "US",
            CallingCode = "+1",
            StatesRequired = true,
            IsActive = true
        };

        await SetupCountriesAsync([country], TestContext.Current.CancellationToken);

        var parameters = new GetCountryPagedOrAll.Parameters { PageNumber = 1, PageSize = 10 };

        var result = await _handler.Handle(
            new GetCountryPagedOrAll.Query(parameters),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.ElementAt(0).StatesRequired.Should().BeTrue();
    }

    [Fact(DisplayName = "Should map IsActive property")]
    public async Task Handle_ShouldMapIsActiveProperty()
    {
        var country = new Country
        {
            Name = "Active Country",
            IsoCode = "AC",
            CallingCode = "+1",
            StatesRequired = false,
            IsActive = true
        };

        await SetupCountriesAsync([country], TestContext.Current.CancellationToken);

        var parameters = new GetCountryPagedOrAll.Parameters { PageNumber = 1, PageSize = 10 };

        var result = await _handler.Handle(
            new GetCountryPagedOrAll.Query(parameters),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.ElementAt(0).IsActive.Should().BeTrue();
    }

    #endregion
}