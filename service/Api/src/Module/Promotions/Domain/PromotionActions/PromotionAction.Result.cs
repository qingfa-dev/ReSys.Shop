namespace Module.Promotions.Domain.PromotionActions;

/// <summary>Contains success messages and error factory methods for PromotionAction operations.</summary>
public static class PromotionActionResult
{
    /// <summary>Success message factory for PromotionAction operations.</summary>
    public static class Success
    {
        public static string Created(Guid id) => $"Promotion action with ID '{id}' was successfully created.";
        public static string Updated(Guid id) => $"Promotion action with ID '{id}' was successfully updated.";
        public static string Deleted(Guid id) => $"Promotion action with ID '{id}' was successfully deleted.";
    }

    /// <summary>Error factory methods returning typed Failure instances for PromotionAction operations.</summary>
    public static class Errors
    {
        #region Validation
        /// <summary>Promotion action type is required.</summary>
        public static Error TypeRequired => Error.Validation(
            code: "PromotionAction.Type.Required",
            description: "Promotion action type is required.");

        public static Error TypeTooLong => Error.Validation(
            code: "PromotionAction.Type.TooLong",
            description: $"Promotion action type cannot exceed {PromotionActionConstant.Constraints.MaxTypeLength} characters.");

        public static Error InvalidType => Error.Validation(
            code: "PromotionAction.Type.Invalid",
            description: "Promotion action type is invalid.");

        public static Error CalculatorTypeTooLong => Error.Validation(
            code: "PromotionAction.CalculatorType.TooLong",
            description: $"Calculator type cannot exceed {PromotionActionConstant.Constraints.MaxCalculatorTypeLength} characters.");
        #endregion Validation

        #region Business
        public static Error NotFound(Guid id) => Error.NotFound(
            code: "PromotionAction.NotFound",
            description: $"Promotion action with ID '{id}' was not found.");

        public static Error PreferenceNotFound(string key) => Error.NotFound(
            code: "PromotionAction.Preference.NotFound",
            description: $"Promotion action preference with key '{key}' was not found.");
        #endregion Business
    }
}