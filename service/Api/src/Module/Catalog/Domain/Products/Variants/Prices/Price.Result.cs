namespace Module.Catalog.Domain.Products.Variants.Prices;

public static class PriceResult
{
    public static class Success
    {
        /// <summary>Returns a success message for price creation.</summary>
        public static string Created(Guid id) => $"Price with ID '{id}' was successfully created.";
        /// <summary>Returns a success message for price update.</summary>
        public static string Updated(Guid id) => $"Price with ID '{id}' was successfully updated.";
        /// <summary>Returns a success message for price deletion.</summary>
        public static string Deleted(Guid id) => $"Price with ID '{id}' was successfully deleted.";
        /// <summary>Compare-at price was successfully updated.</summary>
        public static string CompareAtUpdated => "Compare-at price was successfully updated.";
        /// <summary>Price was successfully marked as default.</summary>
        public static string MarkedAsDefault => "Price was successfully marked as default.";
    }

    public static class Errors
    {
        #region Validation
        /// <summary>Currency is required.</summary>
        public static Error  CurrencyRequired => Error.Validation(
            code: "Price.Currency.Required",
            message: "Currency is required.");

        /// <summary>Price amount must be greater than or equal to zero.</summary>
        public static Error  InvalidAmount => Error.Validation(
            code: "Price.Amount.Invalid",
            message: "Price amount must be greater than or equal to zero.");

        /// <summary>Compare-at amount must be greater than or equal to zero.</summary>
        public static Error  InvalidCompareAtAmount => Error.Validation(
            code: "Price.CompareAtAmount.Invalid",
            message: "Compare at amount must be greater than or equal to zero.");

        /// <summary>Currency exceeds the maximum length.</summary>
        public static Error  CurrencyTooLong => Error.Validation(
            code: "Price.Currency.TooLong",
            message: $"Currency cannot exceed {PriceConstant.Constraints.CurrencyMaxLength} characters.");

        /// <summary>Country ISO code is invalid.</summary>
        public static Error  InvalidCountryIso => Error.Validation(
            code: "Price.CountryIso.Invalid",
            message: $"Country ISO code must be {PriceConstant.Constraints.CountryIsoMaxLength} characters.");

        /// <summary>Adjustment type must be Fixed or Percentage.</summary>
        public static Error  InvalidAdjustmentType => Error.Validation(
            code: "Price.AdjustmentType.Invalid",
            message: "Adjustment type must be Fixed or Percentage.");
        #endregion

        #region Business
        /// <summary>Price was not found.</summary>
        public static Error  NotFound => Error.NotFound(
            code: "Price.NotFound",
            message: "Price was not found.");

        /// <summary>Price has already been deleted.</summary>
        public static Error  AlreadyDeleted => Error.Conflict(
            code: "Price.AlreadyDeleted",
            message: "Price has already been deleted.");
        #endregion
    }
}