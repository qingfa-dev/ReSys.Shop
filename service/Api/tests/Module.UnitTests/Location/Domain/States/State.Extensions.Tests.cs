using Module.Location.Domain.States;

namespace Module.UnitTests.Location.Domain.States;

[Trait("Category", "Unit")]
[Trait("Module", "Location")]
[Trait("Entity", "State")]
public class StateExtensionsTests
{
    [Fact(DisplayName = "Create: Should return State with correct properties")]
    public void Create_WithValidParameters_ShouldReturnState()
    {
        var id = Guid.NewGuid();
        var countryId = Guid.NewGuid();

        var result = StateExtensions.Create("California", "CA", countryId, id: id);
        var state = result.Value;

        result.IsSuccess.Should().BeTrue();
        state.Id.Should().Be(id);
        state.Name.Should().Be("California");
        state.Abbreviation.Should().Be("CA");
        state.CountryId.Should().Be(countryId);
        state.IsActive.Should().BeTrue();
        state.CreatedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory(DisplayName = "Create: Should fail when name is empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyName_ShouldReturnFailure(string? name)
    {
        var result = StateExtensions.Create(name!, "CA", Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(StateResult.Failure.NameRequired);
    }

    [Theory(DisplayName = "Create: Should fail when abbreviation is empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyAbbreviation_ShouldReturnFailure(string? abbreviation)
    {
        var result = StateExtensions.Create("California", abbreviation!, Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(StateResult.Failure.AbbreviationRequired);
    }

    [Fact(DisplayName = "Create: Should fail when CountryId is empty")]
    public void Create_WithEmptyCountryId_ShouldReturnFailure()
    {
        var result = StateExtensions.Create("California", "CA", Guid.Empty);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(StateResult.Failure.CountryRequired);
    }

    [Fact(DisplayName = "Create: Should use default IsActive value")]
    public void Create_WithDefaultIsActive_ShouldBeActive()
    {
        var result = StateExtensions.Create("Texas", "TX", Guid.NewGuid());
        var state = result.Value;

        result.IsSuccess.Should().BeTrue();
        state.IsActive.Should().BeTrue();
    }

    [Fact(DisplayName = "Update: Should update properties correctly")]
    public void Update_WithValidParameters_ShouldUpdateProperties()
    {
        var state = StateExtensions.Create("Old", "OL", Guid.NewGuid()).Value;

        var result = state.Update(name: "New", abbreviation: "NW");

        result.IsSuccess.Should().BeTrue();
        state.Name.Should().Be("New");
        state.Abbreviation.Should().Be("NW");
    }

    [Fact(DisplayName = "Update: Partial update should preserve other properties")]
    public void Update_WithSomeNullParams_ShouldPreserveExisting()
    {
        var state = StateExtensions.Create("Original", "OR", Guid.NewGuid()).Value;

        var result = state.Update(name: "Updated");

        result.IsSuccess.Should().BeTrue();
        state.Name.Should().Be("Updated");
        state.Abbreviation.Should().Be("OR");
    }

    [Fact(DisplayName = "Activate: Should activate inactive state")]
    public void Activate_WhenInactive_ShouldSucceed()
    {
        var state = StateExtensions.Create("State", "ST", Guid.NewGuid(), isActive: false).Value;

        var result = state.Activate();

        result.IsSuccess.Should().BeTrue();
        state.IsActive.Should().BeTrue();
    }

    [Fact(DisplayName = "Activate: When already active should return Ok idempotently")]
    public void Activate_WhenAlreadyActive_ShouldReturnOk()
    {
        var state = StateExtensions.Create("State", "ST", Guid.NewGuid()).Value;

        var result = state.Activate();

        result.IsSuccess.Should().BeTrue();
        state.IsActive.Should().BeTrue();
    }

    [Fact(DisplayName = "Deactivate: Should deactivate active state")]
    public void Deactivate_WhenActive_ShouldSucceed()
    {
        var state = StateExtensions.Create("State", "ST", Guid.NewGuid()).Value;

        var result = state.Deactivate();

        result.IsSuccess.Should().BeTrue();
        state.IsActive.Should().BeFalse();
    }

    [Fact(DisplayName = "Deactivate: When already inactive should return Ok idempotently")]
    public void Deactivate_WhenAlreadyInactive_ShouldReturnOk()
    {
        var state = StateExtensions.Create("State", "ST", Guid.NewGuid(), isActive: false).Value;

        var result = state.Deactivate();

        result.IsSuccess.Should().BeTrue();
        state.IsActive.Should().BeFalse();
    }
}
