using Module.Location.Domain.Countries;
using Module.Location.Features.Shared.Countries.Mappings;
using Module.Location.Features.Shared.Countries.Models;

namespace Module.UnitTests.Location.Features.Admin.Countries.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Location")]
[Trait("Feature", "Mappings")]
public class CountryModelMappingTests
{
    private static Country CreateCountry(Action<Country>? configure = null)
    {
        var country = new Country
        {
            Name = "Korea",
            IsoCode = "KR",
            CallingCode = "+82",
            StatesRequired = true,
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1),
            ModifiedAtUtc = DateTimeOffset.UtcNow.AddDays(-1),
            CreatedBy = "admin",
            ModifiedBy = "admin"
        };
        configure?.Invoke(country);
        return country;
    }

    [Fact(DisplayName = "Mapping: ToDetail maps all properties including auditable fields")]
    public void ToDetail_WhenCalled_MapsAllPropertiesToDetailResponse()
    {
        var country = CreateCountry();

        var response = country.MapToDetail<CountryDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(country.Id);
        response.Name.Should().Be(country.Name);
        response.IsoCode.Should().Be(country.IsoCode);
        response.CallingCode.Should().Be(country.CallingCode);
        response.StatesRequired.Should().Be(country.StatesRequired);
        response.IsActive.Should().Be(country.IsActive);
        response.CreatedAtUtc.Should().Be(country.CreatedAtUtc);
        response.ModifiedAtUtc.Should().Be(country.ModifiedAtUtc);
        response.CreatedBy.Should().Be(country.CreatedBy);
        response.ModifiedBy.Should().Be(country.ModifiedBy);
    }

    [Fact(DisplayName = "Mapping: ToDetail handles null auditable fields")]
    public void ToDetail_WhenAuditableFieldsAreNull_MapsCorrectly()
    {
        var country = CreateCountry(c =>
        {
            c.ModifiedAtUtc = null;
            c.CreatedBy = null;
            c.ModifiedBy = null;
        });

        var response = country.MapToDetail<CountryDetailResponse>();

        response.ModifiedAtUtc.Should().BeNull();
        response.CreatedBy.Should().BeNull();
        response.ModifiedBy.Should().BeNull();
    }

    [Fact(DisplayName = "Mapping: ToListItem maps subset of properties without auditable fields")]
    public void ToListItem_WhenCalled_MapsPropertiesToListItemResponse()
    {
        var country = CreateCountry();

        var response = country.MapToListItem<CountryListItemResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(country.Id);
        response.Name.Should().Be(country.Name);
        response.IsoCode.Should().Be(country.IsoCode);
        response.CallingCode.Should().Be(country.CallingCode);
        response.StatesRequired.Should().Be(country.StatesRequired);
        response.IsActive.Should().Be(country.IsActive);
    }

    [Fact(DisplayName = "Mapping: ToListItem handles null CallingCode")]
    public void ToListItem_WhenCallingCodeIsNull_MapsCorrectly()
    {
        var country = CreateCountry(c => c.CallingCode = null);

        var response = country.MapToListItem<CountryListItemResponse>();

        response.CallingCode.Should().BeNull();
    }
}
