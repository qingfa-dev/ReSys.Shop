namespace Module.Promotions.Domain.PromotionRules;

/// <summary>Contains success messages and error factory methods for PromotionRule operations.</summary>
public static class PromotionRuleResult
{
    /// <summary>Success message factory for PromotionRule operations.</summary>
    public static class Success
    {
        public static string Created(Guid id) => $"Promotion rule with ID '{id}' was successfully created.";
        public static string Updated(Guid id) => $"Promotion rule with ID '{id}' was successfully updated.";
        public static string Deleted(Guid id) => $"Promotion rule with ID '{id}' was successfully deleted.";
    }

    /// <summary>Error factory methods returning typed Failure instances for PromotionRule operations.</summary>
    public static class Errors
    {
        #region Validation
        /// <summary>Promotion rule type is required.</summary>
        public static Error TypeRequired => Error.Validation(
            code: "PromotionRule.Type.Required",
            description: "Promotion rule type is required.");

        public static Error TypeTooLong => Error.Validation(
            code: "PromotionRule.Type.TooLong",
            description: $"Promotion rule type cannot exceed {PromotionRuleConstant.Constraints.MaxTypeLength} characters.");

        public static Error InvalidType => Error.Validation(
            code: "PromotionRule.Type.Invalid",
            description: "Promotion rule type is invalid.");
        #endregion Validation

        #region Business
        public static Error NotFound(Guid id) => Error.NotFound(
            code: "PromotionRule.NotFound",
            description: $"Promotion rule with ID '{id}' was not found.");

        public static Error PreferenceNotFound(string key) => Error.NotFound(
            code: "PromotionRule.Preference.NotFound",
            description: $"Promotion rule preference with key '{key}' was not found.");
        #endregion Business
    }
}