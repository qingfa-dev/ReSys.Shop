using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Shared.Addresses.Mappings;
using Module.Profile.Features.Shared.Addresses.Models;

namespace Module.UnitTests.Profile.Features.Store.Addresses.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "AddressMapping")]
public class AddressMappingTests
{
    [Fact(DisplayName = "ToDomain: Should map request to domain entity")]
    public void ToDomain_ShouldMapRequestToEntity()
    {
        var request = new AddressRequest
        {
            AddressType = AddressType.Shipping,
            FirstName = "John",
            LastName = "Doe",
            Address1 = "123 Main St",
            Address2 = "Apt 4B",
            City = "New York",
            ZipCode = "10001",
            Phone = "+1-555-0100",
            Label = "Home",
            IsDefault = true,
            CountryName = "United States",
            StateProvince = "NY",
            CountryCode = "US",
            StateCode = "NY",
        };

        var address = request.MapToDomain();

        address.Should().NotBeNull();
        address.AddressType.Should().Be(request.AddressType);
        address.FirstName.Should().Be(request.FirstName);
        address.LastName.Should().Be(request.LastName);
        address.Address1.Should().Be(request.Address1);
        address.Address2.Should().Be(request.Address2);
        address.City.Should().Be(request.City);
        address.ZipCode.Should().Be(request.ZipCode);
        address.Phone.Should().Be(request.Phone);
        address.Label.Should().Be(request.Label);
        address.IsDefault.Should().Be(request.IsDefault);
        address.CountryName.Should().Be(request.CountryName);
        address.StateProvince.Should().Be(request.StateProvince);
        address.CountryCode.Should().Be(request.CountryCode);
        address.StateCode.Should().Be(request.StateCode);
    }

    [Fact(DisplayName = "ToDomain: Should handle null optional fields")]
    public void ToDomain_WhenOptionalFieldsAreNull_ShouldMapCorrectly()
    {
        var request = new AddressRequest
        {
            AddressType = AddressType.Billing,
            FirstName = "Jane",
            Address1 = "456 Oak Ave",
            City = "Los Angeles",
            CountryName = "United States",
        };

        var address = request.MapToDomain();

        address.Should().NotBeNull();
        address.FirstName.Should().Be("Jane");
        address.Address1.Should().Be("456 Oak Ave");
        address.City.Should().Be("Los Angeles");
        address.CountryName.Should().Be("United States");
        address.LastName.Should().BeNull();
        address.Address2.Should().BeNull();
        address.ZipCode.Should().BeNull();
        address.Phone.Should().BeNull();
        address.Label.Should().BeNull();
        address.StateProvince.Should().BeNull();
        address.CountryCode.Should().BeNull();
        address.StateCode.Should().BeNull();
    }

    [Fact(DisplayName = "ToResponse: Should map entity to response")]
    public void ToResponse_ShouldMapEntityToResponse()
    {
        var address = CreateAddress();

        var response = address.ToResponse<AddressResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(address.Id);
        response.AddressType.Should().Be(address.AddressType);
        response.FirstName.Should().Be(address.FirstName);
        response.LastName.Should().Be(address.LastName);
        response.Address1.Should().Be(address.Address1);
        response.Address2.Should().Be(address.Address2);
        response.City.Should().Be(address.City);
        response.ZipCode.Should().Be(address.ZipCode);
        response.Phone.Should().Be(address.Phone);
        response.Label.Should().Be(address.Label);
        response.IsDefault.Should().Be(address.IsDefault);
        response.CountryName.Should().Be(address.CountryName);
        response.StateProvince.Should().Be(address.StateProvince);
        response.CountryCode.Should().Be(address.CountryCode);
        response.StateCode.Should().Be(address.StateCode);
    }

    [Fact(DisplayName = "ToResponse: Should handle null optional fields")]
    public void ToResponse_WhenOptionalFieldsAreNull_ShouldMapCorrectly()
    {
        var address = CreateAddress(a =>
        {
            a.LastName = null;
            a.Address2 = null;
            a.ZipCode = null;
            a.Phone = null;
            a.Label = null;
            a.StateProvince = null;
            a.CountryCode = null;
            a.StateCode = null;
        });

        var response = address.ToResponse<AddressResponse>();

        response.LastName.Should().BeNull();
        response.Address2.Should().BeNull();
        response.ZipCode.Should().BeNull();
        response.Phone.Should().BeNull();
        response.Label.Should().BeNull();
        response.StateProvince.Should().BeNull();
        response.CountryCode.Should().BeNull();
        response.StateCode.Should().BeNull();
    }

    [Fact(DisplayName = "UpdateEntity: Should update existing entity from request")]
    public void UpdateEntity_ShouldUpdateAddressFromRequest()
    {
        var address = CreateAddress();
        var originalId = address.Id;

        var request = new AddressRequest
        {
            AddressType = AddressType.Billing,
            FirstName = "Updated",
            LastName = "Name",
            Address1 = "789 New St",
            Address2 = "Suite 200",
            City = "Chicago",
            ZipCode = "60601",
            Phone = "+1-555-0200",
            Label = "Work",
            IsDefault = false,
            CountryName = "Canada",
            StateProvince = "ON",
            CountryCode = "CA",
            StateCode = "ON",
        };

        address.UpdateEntity(request);

        address.Id.Should().Be(originalId);
        address.AddressType.Should().Be(request.AddressType);
        address.FirstName.Should().Be(request.FirstName);
        address.LastName.Should().Be(request.LastName);
        address.Address1.Should().Be(request.Address1);
        address.Address2.Should().Be(request.Address2);
        address.City.Should().Be(request.City);
        address.ZipCode.Should().Be(request.ZipCode);
        address.Phone.Should().Be(request.Phone);
        address.Label.Should().Be(request.Label);
        address.IsDefault.Should().Be(request.IsDefault);
        address.CountryName.Should().Be(request.CountryName);
        address.StateProvince.Should().Be(request.StateProvince);
        address.CountryCode.Should().Be(request.CountryCode);
        address.StateCode.Should().Be(request.StateCode);
    }

    private static Address CreateAddress(Action<Address>? configure = null)
    {
        var address = new Address
        {
            Id = Guid.NewGuid(),
            AddressType = AddressType.Shipping,
            FirstName = "John",
            LastName = "Doe",
            Address1 = "123 Main St",
            Address2 = "Apt 4B",
            City = "New York",
            ZipCode = "10001",
            Phone = "+1-555-0100",
            Label = "Home",
            IsDefault = true,
            CountryName = "United States",
            StateProvince = "NY",
            CountryCode = "US",
            StateCode = "NY",
        };
        configure?.Invoke(address);
        return address;
    }
}
