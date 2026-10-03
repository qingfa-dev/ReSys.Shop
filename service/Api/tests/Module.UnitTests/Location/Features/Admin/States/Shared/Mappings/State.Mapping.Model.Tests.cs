using Module.Location.Domain.Countries;
using Module.Location.Domain.States;
using Module.Location.Features.Shared.States.Mappings;
using Module.Location.Features.Shared.States.Models;

namespace Module.UnitTests.Location.Features.Admin.States.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Location")]
[Trait("Feature", "Mappings")]
public class StateModelMappingTests
{
    private static State CreateState(Action<State>? configure = null)
    {
        var country = new Country
        {
            Name = "United States"
        };

        var state = new State
        {
            Name = "California",
            Abbreviation = "CA",
            CountryId = country.Id,
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1),
            ModifiedAtUtc = DateTimeOffset.UtcNow.AddDays(-1),
            CreatedBy = "admin",
            ModifiedBy = "admin",
            Country = country
        };
        configure?.Invoke(state);
        return state;
    }

    [Fact(DisplayName = "Mapping: ToDetail maps all properties including auditable fields")]
    public void ToDetail_WhenCalled_MapsAllPropertiesToStateResponse()
    {
        var state = CreateState();

        var response = state.MapToDetail<StateDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(state.Id);
        response.Name.Should().Be(state.Name);
        response.Abbreviation.Should().Be(state.Abbreviation);
        response.CountryId.Should().Be(state.CountryId);
        response.IsActive.Should().Be(state.IsActive);
        response.CreatedAtUtc.Should().Be(state.CreatedAtUtc);
        response.ModifiedAtUtc.Should().Be(state.ModifiedAtUtc);
        response.CreatedBy.Should().Be(state.CreatedBy);
        response.ModifiedBy.Should().Be(state.ModifiedBy);
    }

    [Fact(DisplayName = "Mapping: ToListItem maps all properties with CountryName from navigation")]
    public void ToListItem_WhenCalledWithCountry_MapsCountryName()
    {
        var state = CreateState();

        var response = state.MapToListItem<StateListResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(state.Id);
        response.Name.Should().Be(state.Name);
        response.Abbreviation.Should().Be(state.Abbreviation);
        response.CountryId.Should().Be(state.CountryId);
        response.CountryName.Should().Be("United States");
        response.IsActive.Should().Be(state.IsActive);
    }

    [Fact(DisplayName = "Mapping: ToListItem handles null Country navigation")]
    public void ToListItem_WhenCountryIsNull_CountryNameIsNull()
    {
        var state = CreateState(s => s.Country = null!);

        var response = state.MapToListItem<StateListResponse>();

        response.CountryName.Should().BeNull();
    }
}
