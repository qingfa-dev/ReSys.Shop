namespace Module.Promotions.Domain.PromotionRules;

public static class PromotionRuleConstant
{
    public static class Constraints
    {
        public const int MaxTypeLength = 100;
    }

    public static class Types
    {
        public const string ItemTotal = "ItemTotal";
        public const string Product = "Product";
        public const string Taxon = "Taxon";
        public const string User = "User";
        public const string UserRole = "UserRole";
        public const string UserGroup = "UserGroup";
        public const string ShippingCountry = "ShippingCountry";
        public const string ShippingMethod = "ShippingMethod";
        public const string PaymentMethod = "PaymentMethod";
        public const string Store = "Store";

        public static readonly string[] All =
        [
            ItemTotal, Product, Taxon, User, UserRole, UserGroup,
            ShippingCountry, ShippingMethod, PaymentMethod, Store
        ];
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(PromotionRule.Type)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(PromotionRule.Type),
            nameof(PromotionRule.CreatedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(PromotionRule.Type),
            nameof(PromotionRule.PromotionId),
            nameof(PromotionRule.CreatedAtUtc)
        ];
    }
}