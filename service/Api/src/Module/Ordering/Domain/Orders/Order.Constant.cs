using Shared.Application.Domain.Currencies;

namespace Module.Ordering.Domain.Orders;

// Initialize: Default constraints and query configuration for Order entity
public static class OrderConstant
{
    public static class Constraints
    {
        public const int MaxNumberLength = 50;
        public const int MaxSessionIdLength = 100;
        public const int MaxCurrencyLength = SystemCurrencyConstant.Constraints.MaxCodeLength;
        public const int MaxEmailLength = 255;
        public const int MaxSpecialInstructionsLength = 2000;
        public const int Precision = SystemCurrencyConstant.Constraints.MonetaryPrecision;
        public const int Scale = SystemCurrencyConstant.Constraints.MonetaryScale;
        public const int MaxLineItems = 100;
        public const int MaxAdjustments = 50;
    }

    public static class Defaults
    {
        public const string Currency = SystemCurrencyConstant.Defaults.Code;
        public const string CreatedBy = "System";
    }

    public static class CancelReasons
    {
        public const string Customer = "Order cancelled by customer";
        public const string Admin = "Order cancelled by admin";
    }

    public static class StockAction
    {
        public const string Ship = "ship";
        public const string Return = "return";
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(Order.Number),
            nameof(Order.Email)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(Order.Number),
            nameof(Order.Total),
            nameof(Order.CompletedAtUtc),
            nameof(Order.CreatedAtUtc),
            nameof(Order.Status)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(Order.Status),
            nameof(Order.CheckoutState),
            nameof(Order.Currency),
            nameof(Order.UserId),
            nameof(Order.IsDeleted)
        ];
    }
}