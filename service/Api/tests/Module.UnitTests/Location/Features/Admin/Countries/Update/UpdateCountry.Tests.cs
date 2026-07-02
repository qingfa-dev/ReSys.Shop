using Module.Location.Domain.Countries;
using Module.Location.Features.Admin.Countries.Update;

using Shared.Operational.Persistence.Data;

namespace Module.UnitTests.Location.Features.Admin.Countries.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Location")]
[Trait("Feature", "CountryUpdate")]
public class UpdateCountryTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UpdateCountry.CommandHandler _handler;

    public UpdateCountryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Country).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new UpdateCountry.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Should update country successfully")]
    public async Task Handle_ShouldReturnSuccess_WhenCountryExists()
    {
        // Arrange
        var country = new Country { Name = "United States", IsoCode = "US", CallingCode = "+1" };
        _dbContext.Set<Country>().Add(country);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateCountry.Request
        {
            Name = "United States of America", IsoCode = "US", CallingCode = "+1"
        };

        // Act
        var result = await _handler.Handle(
            new UpdateCountry.Command(country.Id, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("United States of America");
        result.Value.IsoCode.Should().Be("US");
    }

    [Fact(DisplayName = "Should return NotFound when country doesn't exist")]
    public async Task Handle_ShouldReturnFailure_WhenCountryNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new UpdateCountry.Request { Name = "Test", IsoCode = "TT" };

        // Act
        var result = await _handler.Handle(
            new UpdateCountry.Command(nonExistentId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(CountryResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Should return duplicate error when IsoCode belongs to another country")]
    public async Task Handle_ShouldReturnFailure_WhenIsoCodeIsDuplicate()
    {
        // Arrange
        var country = new Country { Name = "USA", IsoCode = "US" };
        var otherCountry = new Country { Name = "Canada", IsoCode = "CA" };
        _dbContext.Set<Country>().Add(country);
        _dbContext.Set<Country>().Add(otherCountry);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateCountry.Request { Name = "Trying US", IsoCode = "CA" };

        // Act
        var result = await _handler.Handle(
            new UpdateCountry.Command(country.Id, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(CountryResult.Failure.IsoCodeDuplicate.Code);
    }

    [Fact(DisplayName = "Should set ModifiedAt timestamp")]
    public async Task Handle_ShouldSetModifiedTimestamp()
    {
        // Arrange
        var country = new Country { Name = "Test", IsoCode = "TT" };
        _dbContext.Set<Country>().Add(country);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateCountry.Request { Name = "Updated", IsoCode = "TT" };

        // Act
        var result = await _handler.Handle(
            new UpdateCountry.Command(country.Id, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.Value.ModifiedAtUtc.Should().NotBeNull();
        result.Value.ModifiedAtUtc.Should().NotBe(default);
    }

    [Fact(DisplayName = "Should allow same IsoCode (current country unchanged)")]
    public async Task Handle_ShouldAllowSameIsoCode()
    {
        // Arrange
        var country = new Country { Name = "USA", IsoCode = "US" };
        _dbContext.Set<Country>().Add(country);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateCountry.Request { Name = "United States", IsoCode = "US" };

        // Act
        var result = await _handler.Handle(
            new UpdateCountry.Command(country.Id, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsoCode.Should().Be("US");
    }

    [Fact(DisplayName = "Should update all properties")]
    public async Task Handle_ShouldUpdateAllProperties()
    {
        // Arrange
        var country = new Country
        {
            Name = "Old Name",
            IsoCode = "ON",
            CallingCode = "+111",
            StatesRequired = false,
            IsActive = true
        };
        _dbContext.Set<Country>().Add(country);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateCountry.Request
        {
            Name = "New Name",
            IsoCode = "NN",
            CallingCode = "+222",
            StatesRequired = true,
            IsActive = false,
        };

        // Act
        var result = await _handler.Handle(
            new UpdateCountry.Command(country.Id, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.Value.Name.Should().Be("New Name");
        result.Value.IsoCode.Should().Be("NN");
        result.Value.CallingCode.Should().Be("+222");
        result.Value.StatesRequired.Should().BeTrue();
        result.Value.IsActive.Should().BeFalse();
    }

    [Fact(DisplayName = "Should persist changes to database")]
    public async Task Handle_ShouldPersistChanges_ToDatabase()
    {
        // Arrange
        var country = new Country { Name = "Test", IsoCode = "TT" };
        _dbContext.Set<Country>().Add(country);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var countryId = country.Id;
        var request = new UpdateCountry.Request { Name = "Updated", IsoCode = "TT" };

        // Act
        await _handler.Handle(
            new UpdateCountry.Command(countryId, request),
            TestContext.Current.CancellationToken);

        // Assert
        var updatedCountry = await _dbContext.Set<Country>()
            .FirstOrDefaultAsync(c => c.Id == countryId, TestContext.Current.CancellationToken);

        updatedCountry.Should().NotBeNull();
        updatedCountry.Name.Should().Be("Updated");
        updatedCountry.ModifiedAtUtc.Should().NotBeNull();
    }
}