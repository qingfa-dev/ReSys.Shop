namespace Module.Promotions.Domain.PromotionCategories;

/// <summary>Contains success messages and error factory methods for PromotionCategory operations.</summary>
public static class PromotionCategoryResult
{
    /// <summary>Success message factory for PromotionCategory operations.</summary>
    public static class Success
    {
        public static string Created(Guid id) => $"Promotion category with ID '{id}' was successfully created.";
        public static string Updated(Guid id) => $"Promotion category with ID '{id}' was successfully updated.";
        public static string Deleted(Guid id) => $"Promotion category with ID '{id}' was successfully deleted.";
    }

    /// <summary>Error factory methods returning typed Failure instances for PromotionCategory operations.</summary>
    public static class Errors
    {
        #region Validation
        /// <summary>Promotion category name is required.</summary>
        public static Error NameRequired => Error.Validation(
            code: "PromotionCategory.Name.Required",
            description: "Promotion category name is required.");

        public static Error NameTooLong => Error.Validation(
            code: "PromotionCategory.Name.TooLong",
            description: $"Promotion category name cannot exceed {PromotionCategoryConstant.Constraints.MaxNameLength} characters.");

        public static Error CodeTooLong => Error.Validation(
            code: "PromotionCategory.Code.TooLong",
            description: $"Promotion category code cannot exceed {PromotionCategoryConstant.Constraints.MaxCodeLength} characters.");

        public static Error PresentationTooLong => Error.Validation(
            code: "PromotionCategory.Presentation.TooLong",
            description: $"Presentation cannot exceed {PromotionCategoryConstant.Constraints.MaxPresentationLength} characters.");
        #endregion Validation

        #region Business
        public static Error NotFound(Guid id) => Error.NotFound(
            code: "PromotionCategory.NotFound",
            description: $"Promotion category with ID '{id}' was not found.");
        #endregion Business
    }
}