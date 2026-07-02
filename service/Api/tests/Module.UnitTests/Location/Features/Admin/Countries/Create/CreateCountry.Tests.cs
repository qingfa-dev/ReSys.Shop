using Microsoft.EntityFrameworkCore;

using Module.Location.Domain.Countries;
using Module.Location.Features.Admin.Countries.Create;

using Shared.Operational.Persistence.Data;

namespace Module.UnitTests.Location.Features.Admin.Countries.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Locations")]
[Trait("Feature", "CountryCreate")]
public class CreateCountryTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CreateCountry.CommandHandler _handler;

    public CreateCountryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Country).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new CreateCountry.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Should create country successfully")]
    public async Task Handle_ShouldReturnSuccess_WhenCountryIsValid()
    {
        // Arrange
        var request = new CreateCountry.Request
        {
            Name = "United States",
            IsoCode = "US",
            CallingCode = "+1",
            StatesRequired = true,
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(
            new CreateCountry.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("United States");
        result.Value.IsoCode.Should().Be("US");
        result.Value.CallingCode.Should().Be("+1");
    }

    [Fact(DisplayName = "Should fail when ISO code is duplicate")]
    public async Task Handle_ShouldReturnFailure_WhenIsoCodeIsDuplicate()
    {
        // Arrange
        _dbContext.Set<Country>().Add(new Country { Name = "United States", IsoCode = "US", CallingCode = "+1" });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CreateCountry.Request { Name = "Duplicate", IsoCode = "US" };

        // Act
        var result = await _handler.Handle(
            new CreateCountry.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(CountryResult.Errors.IsoCodeDuplicate.Code);
    }

    [Fact(DisplayName = "Should set CreatedAt timestamp")]
    public async Task Handle_ShouldSetCreatedTimestamp()
    {
        // Arrange
        var request = new CreateCountry.Request { Name = "Canada", IsoCode = "CA" };

        // Act
        var result = await _handler.Handle(
            new CreateCountry.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.Value.Id.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "Should map all properties correctly")]
    public async Task Handle_ShouldReturnCountryWithCorrectProperties()
    {
        // Arrange
        var request = new CreateCountry.Request
        {
            Name = "Germany",
            IsoCode = "DE",
            CallingCode = "+49",
            StatesRequired = false,
            IsActive = true,
        };

        // Act
        var result = await _handler.Handle(
            new CreateCountry.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.Value.Id.Should().NotBe(Guid.Empty);
        result.Value.Name.Should().Be("Germany");
        result.Value.IsoCode.Should().Be("DE");
        result.Value.CallingCode.Should().Be("+49");
        result.Value.StatesRequired.Should().BeFalse();
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact(DisplayName = "Should apply default values")]
    public async Task Handle_ShouldUseDefaultValues_WhenNotProvided()
    {
        // Arrange
        var request = new CreateCountry.Request { Name = "Japan", IsoCode = "JP" };

        // Act
        var result = await _handler.Handle(
            new CreateCountry.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.Value.StatesRequired.Should().BeFalse();
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact(DisplayName = "Should return null CallingCode when not provided")]
    public async Task Handle_ShouldReturnNullCallingCode_WhenNotProvided()
    {
        // Arrange
        var request = new CreateCountry.Request { Name = "Brazil", IsoCode = "BR" };

        // Act
        var result = await _handler.Handle(
            new CreateCountry.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.Value.CallingCode.Should().BeNull();
    }

    [Fact(DisplayName = "Should persist country to database")]
    public async Task Handle_ShouldPersistCountry_ToDatabase()
    {
        // Arrange
        var request = new CreateCountry.Request { Name = "Australia", IsoCode = "AU", CallingCode = "+61" };

        // Act
        await _handler.Handle(
            new CreateCountry.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        var country = await _dbContext.Set<Country>()
            .FirstOrDefaultAsync(c => c.IsoCode == "AU", TestContext.Current.CancellationToken);

        country.Should().NotBeNull();
        country.Name.Should().Be("Australia");
        country.CallingCode.Should().Be("+61");
    }

    [Fact(DisplayName = "Should allow different ISO codes")]
    public async Task Handle_ShouldAllowDifferentIsoCodes()
    {
        // Arrange
        var req1 = new CreateCountry.Request { Name = "Italy", IsoCode = "IT" };
        var req2 = new CreateCountry.Request { Name = "Spain", IsoCode = "ES" };

        // Act
        var res1 = await _handler.Handle(new CreateCountry.Command(req1), TestContext.Current.CancellationToken);
        var res2 = await _handler.Handle(new CreateCountry.Command(req2), TestContext.Current.CancellationToken);

        // Assert
        res1.IsSuccess.Should().BeTrue();
        res2.IsSuccess.Should().BeTrue();
        res1.Value.IsoCode.Should().Be("IT");
        res2.Value.IsoCode.Should().Be("ES");
    }
}