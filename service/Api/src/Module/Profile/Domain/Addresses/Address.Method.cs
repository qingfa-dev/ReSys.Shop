using Microsoft.CodeAnalysis;

namespace Module.Profile.Domain.Addresses;

public static class AddressMethod
{
    #region Factory Methods

    public static Result<Address> Create(
        string firstName,
        string address1,
        string city,
        string countryName,
        AddressType addressType = AddressType.Shipping,
        string? lastName = null,
        string? address2 = null,
        string? zipCode = null,
        string? phone = null,
        string? label = null,
        bool isDefault = false,
        bool isDefaultBilling = false,
        bool isDefaultShipping = false,
        string? stateProvince = null,
        string? countryCode = null,
        string? stateCode = null,
        Guid? userProfileId = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return AddressResult.Failure.FirstNameRequired;
        if (firstName.Length > AddressConstant.Constraints.MaxFirstNameLength)
            return AddressResult.Failure.FirstNameTooLong;

        if (string.IsNullOrWhiteSpace(address1))
            return AddressResult.Failure.Address1Required;
        if (address1.Length > AddressConstant.Constraints.MaxAddress1Length)
            return AddressResult.Failure.Address1TooLong;

        if (string.IsNullOrWhiteSpace(city))
            return AddressResult.Failure.CityRequired;
        if (city.Length > AddressConstant.Constraints.MaxCityLength)
            return AddressResult.Failure.CityTooLong;

        if (string.IsNullOrWhiteSpace(countryName))
            return AddressResult.Failure.CountryNameRequired;
        if (countryName.Length > AddressConstant.Constraints.MaxCountryNameLength)
            return AddressResult.Failure.CountryNameTooLong;

        if (lastName?.Length > AddressConstant.Constraints.MaxLastNameLength)
            return AddressResult.Failure.LastNameTooLong;
        if (address2?.Length > AddressConstant.Constraints.MaxAddress2Length)
            return AddressResult.Failure.Address2TooLong;
        if (zipCode?.Length > AddressConstant.Constraints.MaxZipCodeLength)
            return AddressResult.Failure.ZipCodeTooLong;
        if (phone?.Length > AddressConstant.Constraints.MaxPhoneLength)
            return AddressResult.Failure.PhoneTooLong;
        if (label?.Length > AddressConstant.Constraints.MaxLabelLength)
            return AddressResult.Failure.LabelTooLong;
        if (stateProvince?.Length > AddressConstant.Constraints.MaxStateProvinceLength)
            return AddressResult.Failure.StateProvinceTooLong;
        if (countryCode?.Length > AddressConstant.Constraints.MaxCountryCodeLength)
            return AddressResult.Failure.CountryCodeTooLong;
        if (stateCode?.Length > AddressConstant.Constraints.MaxStateCodeLength)
            return AddressResult.Failure.StateCodeTooLong;
        if (userProfileId.HasValue && userProfileId.Value == Guid.Empty)
            return AddressResult.Failure.UserProfileIdRequired;

        return new Address
        {
            AddressType = addressType,
            FirstName = firstName,
            LastName = lastName,
            Address1 = address1,
            Address2 = address2,
            City = city,
            ZipCode = zipCode,
            Phone = phone,
            Label = label,
            IsDefault = isDefault,
            IsDefaultBilling = isDefaultBilling,
            IsDefaultShipping = isDefaultShipping,
            CountryName = countryName,
            StateProvince = stateProvince,
            CountryCode = countryCode,
            StateCode = stateCode,
            UserProfileId = userProfileId
        };
    }

    #endregion

    #region Update

    public static Result<Address> Update(
        this Address address,
        Optional<string?> firstName = default,
        Optional<string?> lastName = default,
        Optional<string?> address1 = default,
        Optional<string?> address2 = default,
        Optional<string?> city = default,
        Optional<string?> zipCode = default,
        Optional<string?> phone = default,
        Optional<string?> label = default,
        Optional<bool> isDefault = default,
        Optional<bool> isDefaultBilling = default,
        Optional<bool> isDefaultShipping = default,
        Optional<string?> stateProvince = default,
        Optional<string?> countryCode = default,
        Optional<string?> stateCode = default,
        Optional<string?> countryName = default,
        Optional<AddressType> addressType = default)
    {
        if (firstName.HasValue)
        {
            if (firstName.Value?.Length > AddressConstant.Constraints.MaxFirstNameLength)
                return AddressResult.Failure.FirstNameTooLong;
            address.FirstName = firstName.Value!;
        }

        if (lastName.HasValue)
        {
            if (lastName.Value?.Length > AddressConstant.Constraints.MaxLastNameLength)
                return AddressResult.Failure.LastNameTooLong;
            address.LastName = lastName.Value;
        }

        if (address1.HasValue)
        {
            if (address1.Value?.Length > AddressConstant.Constraints.MaxAddress1Length)
                return AddressResult.Failure.Address1TooLong;
            address.Address1 = address1.Value!;
        }

        if (address2.HasValue)
        {
            if (address2.Value?.Length > AddressConstant.Constraints.MaxAddress2Length)
                return AddressResult.Failure.Address2TooLong;
            address.Address2 = address2.Value;
        }

        if (city.HasValue)
        {
            if (city.Value?.Length > AddressConstant.Constraints.MaxCityLength)
                return AddressResult.Failure.CityTooLong;
            address.City = city.Value!;
        }

        if (zipCode.HasValue)
        {
            if (zipCode.Value?.Length > AddressConstant.Constraints.MaxZipCodeLength)
                return AddressResult.Failure.ZipCodeTooLong;
            address.ZipCode = zipCode.Value;
        }

        if (phone.HasValue)
        {
            if (phone.Value?.Length > AddressConstant.Constraints.MaxPhoneLength)
                return AddressResult.Failure.PhoneTooLong;
            address.Phone = phone.Value;
        }

        if (label.HasValue)
        {
            if (label.Value?.Length > AddressConstant.Constraints.MaxLabelLength)
                return AddressResult.Failure.LabelTooLong;
            address.Label = label.Value;
        }

        if (isDefault.HasValue)
            address.IsDefault = isDefault.Value;

        if (isDefaultBilling.HasValue)
            address.IsDefaultBilling = isDefaultBilling.Value;

        if (isDefaultShipping.HasValue)
            address.IsDefaultShipping = isDefaultShipping.Value;

        if (stateProvince.HasValue)
        {
            if (stateProvince.Value?.Length > AddressConstant.Constraints.MaxStateProvinceLength)
                return AddressResult.Failure.StateProvinceTooLong;
            address.StateProvince = stateProvince.Value;
        }

        if (countryCode.HasValue)
        {
            if (countryCode.Value?.Length > AddressConstant.Constraints.MaxCountryCodeLength)
                return AddressResult.Failure.CountryCodeTooLong;
            address.CountryCode = countryCode.Value;
        }

        if (stateCode.HasValue)
        {
            if (stateCode.Value?.Length > AddressConstant.Constraints.MaxStateCodeLength)
                return AddressResult.Failure.StateCodeTooLong;
            address.StateCode = stateCode.Value;
        }

        if (countryName.HasValue)
        {
            if (countryName.Value?.Length > AddressConstant.Constraints.MaxCountryNameLength)
                return AddressResult.Failure.CountryNameTooLong;
            address.CountryName = countryName.Value!;
        }

        if (addressType.HasValue)
            address.AddressType = addressType.Value;

        return address;
    }

    #endregion

    #region Business Logic

    public static void MarkAsDefault(this Address address)
    {
        address.IsDefault = true;
    }

    public static string FullName(this Address address)
    {
        return string.IsNullOrWhiteSpace(address.LastName)
            ? address.FirstName
            : $"{address.FirstName} {address.LastName}";
    }

    public static string FullAddress(this Address address)
    {
        List<string> lines = [];

        lines.Add(address.FullName());
        lines.Add(address.Address1);

        if (!string.IsNullOrWhiteSpace(address.Address2))
            lines.Add(address.Address2);

        string cityLine = address.City;
        if (!string.IsNullOrWhiteSpace(address.StateProvince))
            cityLine += $", {address.StateProvince}";
        if (!string.IsNullOrWhiteSpace(address.ZipCode))
            cityLine += $" {address.ZipCode}";
        lines.Add(cityLine);

        if (!string.IsNullOrWhiteSpace(address.CountryName))
            lines.Add(address.CountryName);

        return string.Join("\n", lines);
    }

    #endregion
}