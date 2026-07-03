namespace Module.Catalog.Domain.Products.Variants.Prices;

public static class PriceHistoryResult
{
    public static class Success
    {
        public static string Created(Guid id) => $"Price history entry with ID '{id}' was successfully created.";
    }

    public static class Errors
    {
        public static Error  AmountRequired => Error.Validation(
            code: "PriceHistory.Amount.Required",
            message: "Price history amount is required.");

        public static Error  InvalidAmount => Error.Validation(
            code: "PriceHistory.Amount.Invalid",
            message: $"Price history amount must be greater than or equal to {PriceHistoryConstant.Constraints.MinAmount}.");

        public static Error  CurrencyRequired => Error.Validation(
            code: "PriceHistory.Currency.Required",
            message: "Currency is required.");

        public static Error  NotFound => Error.NotFound(
            code: "PriceHistory.NotFound",
            message: "Price history entry was not found.");
    }
}
