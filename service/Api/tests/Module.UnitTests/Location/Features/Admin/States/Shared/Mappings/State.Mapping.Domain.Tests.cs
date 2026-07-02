using Module.Location.Domain.States;
using Module.Location.Features.Admin.States.Shared.Mappings;
using Module.Location.Features.Admin.States.Shared.Models;

namespace Module.UnitTests.Location.Features.Admin.States.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Locations")]
[Trait("Feature", "Mappings")]
public class StateDomainMappingTests
{
    [Fact(DisplayName = "Mapping: ToDomain creates new State with all properties mapped")]
    public void ToDomain_WhenCalled_CreatesNewStateWithAllPropertiesMapped()
    {
        var countryId = Guid.NewGuid();
        var request = new StateRequest { Name = "California", Abbreviation = "CA", CountryId = countryId, IsActive = true };

        var state = request.MapToDomain();

        state.Should().NotBeNull();
        state.Name.Should().Be(request.Name);
        state.Abbreviation.Should().Be(request.Abbreviation);
        state.CountryId.Should().Be(request.CountryId);
        state.IsActive.Should().Be(request.IsActive);
        state.Id.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "Mapping: ToDomain sets CreatedAtUtc to current time")]
    public void ToDomain_WhenCalled_SetsCreatedAtUtc()
    {
        var request = new StateRequest { Name = "California", Abbreviation = "CA", CountryId = Guid.NewGuid() };

        var state = request.MapToDomain();

        state.CreatedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "Mapping: ToDomain updates existing entity properties")]
    public void ToDomain_WhenUpdatingExistingEntity_UpdatesProperties()
    {
        var originalCreatedAt = DateTimeOffset.UtcNow.AddDays(-1);
        var state = new State
        {
            Name = "OldName",
            Abbreviation = "OO",
            CountryId = Guid.NewGuid(),
            IsActive = false,
            CreatedAtUtc = originalCreatedAt
        };

        var newCountryId = Guid.NewGuid();
        var request = new StateRequest { Name = "California", Abbreviation = "CA", CountryId = newCountryId, IsActive = true };

        request.MapToDomain(state);

        state.Name.Should().Be("California");
        state.Abbreviation.Should().Be("CA");
        state.CountryId.Should().Be(newCountryId);
        state.IsActive.Should().BeTrue();
        state.CreatedAtUtc.Should().Be(originalCreatedAt);
        state.ModifiedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }
}
