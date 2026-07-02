namespace Module.Profile.Domain.Addresses;

public static class AddressResult
{
    public static class Success
    {
        public const string Created = "Address created successfully.";
        public const string Updated = "Address updated successfully.";
        public const string GetList = "Addresses retrieved successfully.";
    }

    public static class Failure
    {
        public static Error NotFound => Error.NotFound(
            code: "Address.NotFound",
            message: "The specified address was not found.");

        public static Error FirstNameRequired => Error.Validation(
            code: "Address.FirstName.Required",
            message: "First name is required.");

        public static Error LastNameRequired => Error.Validation(
            code: "Address.LastName.Required",
            message: "Last name is required.");

        public static Error Address1Required => Error.Validation(
            code: "Address.Address1.Required",
            message: "Street address is required.");

        public static Error CityRequired => Error.Validation(
            code: "Address.City.Required",
            message: "City is required.");

        public static Error AddressTypeRequired => Error.Validation(
            code: "Address.AddressType.Required",
            message: "Address type is required.");

        public static Error AddressTypeInvalid => Error.Validation(
            code: "Address.AddressType.Invalid",
            message: "The specified address type is invalid.");

        public static Error CountryRequired => Error.Validation(
            code: "Address.Country.Required",
            message: "Country is required.");

        public static Error CountryNameRequired => Error.Validation(
            code: "Address.CountryName.Required",
            message: "Country name is required.");

        public static Error CountryNameTooLong => Error.Validation(
            code: "Address.CountryName.TooLong",
            message: $"Country name cannot exceed {AddressConstant.Constraints.MaxCountryNameLength} characters.");

        public static Error FirstNameTooLong => Error.Validation(
            code: "Address.FirstName.TooLong",
            message: $"First name cannot exceed {AddressConstant.Constraints.MaxFirstNameLength} characters.");

        public static Error LastNameTooLong => Error.Validation(
            code: "Address.LastName.TooLong",
            message: $"Last name cannot exceed {AddressConstant.Constraints.MaxLastNameLength} characters.");

        public static Error Address1TooLong => Error.Validation(
            code: "Address.Address1.TooLong",
            message: $"Street address cannot exceed {AddressConstant.Constraints.MaxAddress1Length} characters.");

        public static Error Address2TooLong => Error.Validation(
            code: "Address.Address2.TooLong",
            message: $"Address line 2 cannot exceed {AddressConstant.Constraints.MaxAddress2Length} characters.");

        public static Error CityTooLong => Error.Validation(
            code: "Address.City.TooLong",
            message: $"City cannot exceed {AddressConstant.Constraints.MaxCityLength} characters.");

        public static Error ZipCodeTooLong => Error.Validation(
            code: "Address.ZipCode.TooLong",
            message: $"Zip code cannot exceed {AddressConstant.Constraints.MaxZipCodeLength} characters.");

        public static Error PhoneTooLong => Error.Validation(
            code: "Address.Phone.TooLong",
            message: $"Phone cannot exceed {AddressConstant.Constraints.MaxPhoneLength} characters.");

        public static Error LabelTooLong => Error.Validation(
            code: "Address.Label.TooLong",
            message: $"Label cannot exceed {AddressConstant.Constraints.MaxLabelLength} characters.");

        public static Error StateProvinceTooLong => Error.Validation(
            code: "Address.StateProvince.TooLong",
            message: $"State/Province cannot exceed {AddressConstant.Constraints.MaxStateProvinceLength} characters.");

        public static Error CountryCodeTooLong => Error.Validation(
            code: "Address.CountryCode.TooLong",
            message: $"Country code cannot exceed {AddressConstant.Constraints.MaxCountryCodeLength} characters.");

        public static Error StateCodeTooLong => Error.Validation(
            code: "Address.StateCode.TooLong",
            message: $"State code cannot exceed {AddressConstant.Constraints.MaxStateCodeLength} characters.");

        public static Error MaxAddressesReached => Error.Validation(
            code: "Address.Limit.Total",
            message: "Maximum number of total addresses reached.");

        public static Error MaxAddressesPerTypeReached => Error.Validation(
            code: "Address.Limit.PerType",
            message: "Maximum number of addresses for this type reached.");

        public static Error DuplicateAddress => Error.Validation(
            code: "Address.Duplicate",
            message: "This address already exists in your profile.");

        public static Error UserProfileIdRequired => Error.Validation(
            code: "Address.UserProfileId.Required",
            message: "User profile ID is required.");

        /// <summary>Authentication required for address operations.</summary>
        public static Error AuthRequired => Error.Unauthorized(
            code: "Address.AuthRequired",
            message: "Authentication required.");
    }
}