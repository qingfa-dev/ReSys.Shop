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
        public const string PaymentState = "pending";
        public const string ShipmentState = "pending";
    }

    public static class PaymentState
    {
        public const string Completed = "completed";
        public const string Failed = "failed";
        public const string Void = "void";
        public const string BalanceDue = "balance_due";
        public const string CreditOwed = "credit_owed";
        public const string Paid = "paid";
        public const string Pending = "pending";
        public const string Checkout = "checkout";
        public const string Invalid = "invalid";
    }

    public static class ShipmentState
    {
        public const string Pending = "pending";
        public const string Delivered = "delivered";
        public const string Partial = "partial";
        public const string Ready = "ready";
        public const string Backorder = "backorder";
        public const string Canceled = "canceled";
    }

    public static class CheckoutStep
    {
        public const string Address = "address";
        public const string Delivery = "delivery";
        public const string Payment = "payment";
        public const string Confirm = "confirm";
        public const string Complete = "complete";
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