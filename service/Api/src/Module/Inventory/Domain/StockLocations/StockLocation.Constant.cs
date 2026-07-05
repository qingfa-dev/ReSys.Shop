namespace Module.Inventory.Domain.StockLocations;

public static class StockLocationConstant
{
    public static class Constraints
    {
        public const int NameMaxLength = 255;
        public const int CodeMaxLength = 50;
        public const int AddressMaxLength = 255;
        public const int CityMaxLength = 100;
        public const int PhoneMaxLength = 50;
        public const int PostalCodeMaxLength = 10;
        public const int AdminNameMaxLength = 255;
        public const int PresentationMaxLength = 255;
    }

    public static class Defaults
    {
        public const bool Active = true;
        public const bool Default = false;
        public const bool BackorderableDefault = false;
        public const bool PropagateAllVariants = true;
        public const int Position = 0;
        public const int LowStockThreshold = 5;
        public const bool NotifyOnLowStock = false;
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(StockLocation.Name),
            nameof(StockLocation.Code),
            nameof(StockLocation.City),
            nameof(StockLocation.AdminName)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(StockLocation.Name),
            nameof(StockLocation.Code),
            nameof(StockLocation.Position),
            nameof(StockLocation.CreatedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(StockLocation.Active),
            nameof(StockLocation.Default),
            nameof(StockLocation.BackorderableDefault),
            nameof(StockLocation.IsDeleted),
            nameof(StockLocation.CountryId),
            nameof(StockLocation.StateId)
        ];
    }
}
