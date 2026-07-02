using Module.Location.Domain.Countries;

namespace Module.UnitTests.Location.Domain.Countries;

[Trait("Category", "Unit")]
[Trait("Module", "Locations")]
[Trait("Entity", "Country")]
public class CountryExtensionsTests
{
    [Fact(DisplayName = "Create: Should return Country with correct properties")]
    public void Create_WithValidParameters_ShouldReturnCountry()
    {
        var id = Guid.NewGuid();

        var result = CountryExtensions.Create("United States", "US", "USA", "United States of America", id: id);
        var country = result.Value;

        result.IsSuccess.Should().BeTrue();
        country.Id.Should().Be(id);
        country.Name.Should().Be("United States");
        country.IsoCode.Should().Be("US");
        country.Iso3Code.Should().Be("USA");
        country.IsoName.Should().Be("United States of America");
        country.CallingCode.Should().BeNull();
        country.StatesRequired.Should().BeFalse();
        country.ZipcodeRequired.Should().BeFalse();
        country.IsActive.Should().BeTrue();
        country.CreatedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory(DisplayName = "Create: Should fail when name is empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyName_ShouldReturnFailure(string? name)
    {
        var result = CountryExtensions.Create(name!, "US", "USA", "United States of America");

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(CountryResult.Errors.NameRequired);
    }

    [Theory(DisplayName = "Create: Should fail when ISO code is empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyIsoCode_ShouldReturnFailure(string? isoCode)
    {
        var result = CountryExtensions.Create("United States", isoCode!, "USA", "United States of America");

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(CountryResult.Errors.IsoCodeRequired);
    }

    [Fact(DisplayName = "Create: Should use default values for optional parameters")]
    public void Create_WithDefaultValues_ShouldSetCorrectDefaults()
    {
        var result = CountryExtensions.Create("Canada", "CA", "CAN", "Canada");
        var country = result.Value;

        result.IsSuccess.Should().BeTrue();
        country.CallingCode.Should().BeNull();
        country.StatesRequired.Should().BeFalse();
        country.ZipcodeRequired.Should().BeFalse();
        country.IsActive.Should().BeTrue();
    }

    [Fact(DisplayName = "Update: Should update properties correctly")]
    public void Update_WithValidParameters_ShouldUpdateProperties()
    {
        var country = CountryExtensions.Create("Old", "OL", "OLD", "Old Country").Value;

        var result = country.Update(name: "New", isoName: "New Country", callingCode: "1", statesRequired: true, zipcodeRequired: true);

        result.IsSuccess.Should().BeTrue();
        country.Name.Should().Be("New");
        country.IsoName.Should().Be("New Country");
        country.CallingCode.Should().Be("1");
        country.StatesRequired.Should().BeTrue();
        country.ZipcodeRequired.Should().BeTrue();
    }

    [Fact(DisplayName = "Update: Partial update should preserve other properties")]
    public void Update_WithSomeNullParams_ShouldPreserveExisting()
    {
        var country = CountryExtensions.Create("Country", "CO", "CON", "Original Country").Value;

        var result = country.Update(name: "Updated Name");

        result.IsSuccess.Should().BeTrue();
        country.Name.Should().Be("Updated Name");
        country.IsoName.Should().Be("Original Country");
    }

    [Fact(DisplayName = "Activate: Should activate inactive country")]
    public void Activate_WhenInactive_ShouldSucceed()
    {
        var country = CountryExtensions.Create("Country", "CO", "CON", "Country", isActive: false).Value;

        var result = country.Activate();

        result.IsSuccess.Should().BeTrue();
        country.IsActive.Should().BeTrue();
    }

    [Fact(DisplayName = "Activate: When already active should return Ok idempotently")]
    public void Activate_WhenAlreadyActive_ShouldReturnOk()
    {
        var country = CountryExtensions.Create("Country", "CO", "CON", "Country").Value;

        var result = country.Activate();

        result.IsSuccess.Should().BeTrue();
        country.IsActive.Should().BeTrue();
    }

    [Fact(DisplayName = "Deactivate: Should deactivate active country")]
    public void Deactivate_WhenActive_ShouldSucceed()
    {
        var country = CountryExtensions.Create("Country", "CO", "CON", "Country").Value;

        var result = country.Deactivate();

        result.IsSuccess.Should().BeTrue();
        country.IsActive.Should().BeFalse();
    }

    [Fact(DisplayName = "Deactivate: When already inactive should return Ok idempotently")]
    public void Deactivate_WhenAlreadyInactive_ShouldReturnOk()
    {
        var country = CountryExtensions.Create("Country", "CO", "CON", "Country", isActive: false).Value;

        var result = country.Deactivate();

        result.IsSuccess.Should().BeTrue();
        country.IsActive.Should().BeFalse();
    }

    [Fact(DisplayName = "Deactivate: When country has active states should fail")]
    public void Deactivate_WithActiveStates_ShouldReturnFailure()
    {
        var country = CountryExtensions.Create("Country", "CO", "CON", "Country").Value;
        country.States =
        [
            new() { IsActive = true }
        ];

        var result = country.Deactivate();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(CountryResult.Errors.HasActiveStates);
        country.IsActive.Should().BeTrue();
    }

    [Fact(DisplayName = "HasStates: Should return true when states exist")]
    public void HasStates_WhenStatesExist_ShouldReturnTrue()
    {
        var country = CountryExtensions.Create("Country", "CO", "CON", "Country").Value;
        country.States = [new() { Name = "State1" }];

        var hasStates = country.HasStates();

        hasStates.Should().BeTrue();
    }

    [Fact(DisplayName = "HasStates: Should return false when states list is null")]
    public void HasStates_WhenStatesNull_ShouldReturnFalse()
    {
        var country = CountryExtensions.Create("Country", "CO", "CON", "Country").Value;

        var hasStates = country.HasStates();

        hasStates.Should().BeFalse();
    }
}
