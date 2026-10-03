namespace Module.Customer.Domain.Addresses;

public static class AddressConstant
{
    public static class Defaults
    {
        public const bool IsDefault = false;
    }

    public static class Constraints
    {
        public const int MaxFirstNameLength = 100;
        public const int MaxLastNameLength = 100;
        public const int MaxAddress1Length = 200;
        public const int MaxAddress2Length = 200;
        public const int MaxCityLength = 100;
        public const int MaxZipCodeLength = 20;
        public const int MaxPhoneLength = 20;
        public const int MaxLabelLength = 50;
        public const int MaxCountryNameLength = 100;
        public const int MaxStateProvinceLength = 100;
        public const int MaxCountryCodeLength = 3;
        public const int MaxStateCodeLength = 10;
    }

    /// <summary>
    /// Field metadata for searching, sorting, and filtering.
    /// </summary>
    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(Address.FirstName),
            nameof(Address.LastName),
            nameof(Address.Address1),
            nameof(Address.City),
            nameof(Address.CountryName),
            nameof(Address.Label),
            nameof(Address.Phone)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(Address.FirstName),
            nameof(Address.City),
            nameof(Address.CountryName),
            nameof(Address.AddressType)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(Address.AddressType),
            nameof(Address.CountryCode),
            nameof(Address.StateCode),
            nameof(Address.IsDefault),
            nameof(Address.IsDefaultBilling),
            nameof(Address.IsDefaultShipping),
            nameof(Address.UserProfileId)
        ];
    }
}