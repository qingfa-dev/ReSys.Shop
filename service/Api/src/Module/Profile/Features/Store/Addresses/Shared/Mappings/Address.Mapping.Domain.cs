using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Store.Addresses.Shared.Models;

namespace Module.Profile.Features.Store.Addresses.Shared.Mappings;

public static class AddressMapping
{
    public static T ToResponse<T>(this Address address) where T : AddressResponse, new()
    {
        return new T
        {
            Id = address.Id,
            AddressType = address.AddressType,
            FirstName = address.FirstName,
            LastName = address.LastName,
            Address1 = address.Address1,
            Address2 = address.Address2,
            City = address.City,
            ZipCode = address.ZipCode,
            Phone = address.Phone,
            Label = address.Label,
            IsDefault = address.IsDefault,
            CountryName = address.CountryName,
            StateProvince = address.StateProvince,
            CountryCode = address.CountryCode,
            StateCode = address.StateCode
        };
    }

    public static Address MapToDomain<T>(this T request) where T : AddressRequest
    {
        return AddressMethod.Create(
            request.FirstName,
            request.Address1,
            request.City,
            request.CountryName,
            addressType: request.AddressType,
            lastName: request.LastName,
            address2: request.Address2,
            zipCode: request.ZipCode,
            phone: request.Phone,
            label: request.Label,
            isDefault: request.IsDefault,
            stateProvince: request.StateProvince,
            countryCode: request.CountryCode,
            stateCode: request.StateCode).Value;
    }

    public static void UpdateEntity<T>(this Address address, T request) where T : AddressRequest
    {
        address.AddressType = request.AddressType;
        address.FirstName = request.FirstName;
        address.LastName = request.LastName;
        address.Address1 = request.Address1;
        address.Address2 = request.Address2;
        address.City = request.City;
        address.ZipCode = request.ZipCode;
        address.Phone = request.Phone;
        address.Label = request.Label;
        address.IsDefault = request.IsDefault;
        address.CountryName = request.CountryName;
        address.StateProvince = request.StateProvince;
        address.CountryCode = request.CountryCode;
        address.StateCode = request.StateCode;
    }
}