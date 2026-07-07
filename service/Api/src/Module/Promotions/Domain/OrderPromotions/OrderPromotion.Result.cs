namespace Module.Promotions.Domain.OrderPromotions;

/// <summary>Contains success messages and error factory methods for OrderPromotion operations.</summary>
public static class OrderPromotionResult
{
    /// <summary>Success message factory for OrderPromotion operations.</summary>
    public static class Success
    {
        public static string Created(Guid id) => $"Order promotion with ID '{id}' was successfully created.";
        public static string Deleted(Guid id) => $"Order promotion with ID '{id}' was successfully deleted.";
    }

    public static class Errors
    {
        public static Error NotFound(Guid id) => Error.NotFound(
            code: "OrderPromotion.NotFound",
            description: $"Order promotion with ID '{id}' was not found.");

        public static Error OrderRequired => Error.Validation(
            code: "OrderPromotion.Order.Required",
            description: "Order is required.");

        public static Error PromotionRequired => Error.Validation(
            code: "OrderPromotion.Promotion.Required",
            description: "Promotion is required.");
    }
}