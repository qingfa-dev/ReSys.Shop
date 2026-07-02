using Module.Location.Domain.Countries;
using Module.Location.Features.Admin.Countries.Shared.Mappings;
using Module.Location.Features.Admin.Countries.Shared.Models;

namespace Module.UnitTests.Location.Features.Admin.Countries.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Location")]
[Trait("Feature", "Mappings")]
public class CountryDomainMappingTests
{
    [Fact(DisplayName = "Mapping: ToDomain creates new Country with all properties mapped")]
    public void ToDomain_WhenCalled_CreatesNewCountryWithAllPropertiesMapped()
    {
        var request = new CountryRequest { Name = "Korea", IsoCode = "KR", CallingCode = "+82", StatesRequired = true, IsActive = true };

        var country = request.MapToDomain();

        country.Should().NotBeNull();
        country.Name.Should().Be(request.Name);
        country.IsoCode.Should().Be(request.IsoCode);
        country.CallingCode.Should().Be(request.CallingCode);
        country.StatesRequired.Should().Be(request.StatesRequired);
        country.IsActive.Should().Be(request.IsActive);
        country.Id.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "Mapping: ToDomain invokes AuditableBehavior.Create")]
    public void ToDomain_WhenCalled_InvokesAuditableBehaviorCreate()
    {
        var request = new CountryRequest { Name = "Korea", IsoCode = "KR" };

        var country = request.MapToDomain();

        country.CreatedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        country.ModifiedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "Mapping: ToDomain preserves null CallingCode")]
    public void ToDomain_WhenCallingCodeIsNull_CreatesCountryWithNullCallingCode()
    {
        var country = new CountryRequest { Name = "Korea", IsoCode = "KR", CallingCode = null }.MapToDomain();

        country.CallingCode.Should().BeNull();
    }

    [Fact(DisplayName = "Mapping: ToDomain with existing entity updates properties")]
    public void ToDomain_WhenUpdatingExistingEntity_UpdatesProperties()
    {
        var originalTimestamp = DateTimeOffset.UtcNow.AddDays(-1);
        var country = new Country
        {
            Name = "OldName",
            IsoCode = "OO",
            CallingCode = null,
            StatesRequired = false,
            IsActive = false,
            CreatedAtUtc = originalTimestamp,
            ModifiedAtUtc = originalTimestamp
        };

        var request = new CountryRequest { Name = "Korea", IsoCode = "KR", CallingCode = "+82", StatesRequired = true, IsActive = true };

        request.MapToDomain(country);

        country.Name.Should().Be("Korea");
        country.IsoCode.Should().Be("KR");
        country.CallingCode.Should().Be("+82");
        country.StatesRequired.Should().BeTrue();
        country.IsActive.Should().BeTrue();
        country.CreatedAtUtc.Should().Be(originalTimestamp);
        country.ModifiedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }
}
