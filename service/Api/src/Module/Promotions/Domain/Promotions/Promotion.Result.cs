namespace Module.Promotions.Domain.Promotions;

/// <summary>Contains success messages and error factory methods for Promotion operations.</summary>
public static class PromotionResult
{
    /// <summary>Success message factory for Promotion operations.</summary>
    public static class Success
    {
        /// <summary>Returns a success message for promotion creation.</summary>
        public static string Created(Guid id) => $"Promotion with ID '{id}' was successfully created.";
        /// <summary>Returns a success message for promotion update.</summary>
        public static string Updated(Guid id) => $"Promotion with ID '{id}' was successfully updated.";
        /// <summary>Returns a success message for promotion deletion.</summary>
        public static string Deleted(Guid id) => $"Promotion with ID '{id}' was successfully deleted.";
        /// <summary>Returns a success message for promotion activation.</summary>
        public static string Activated => "Promotion was successfully activated.";
        /// <summary>Returns a success message for promotion deactivation.</summary>
        public static string Deactivated => "Promotion was successfully deactivated.";
    }

    /// <summary>Error factory methods returning typed Failure instances for Promotion operations.</summary>
    public static class Errors
    {
        #region Validation
        /// <summary>Promotion name is required.</summary>
        public static Error NameRequired => Error.Validation(
            code: "Promotion.Name.Required",
            description: "Promotion name is required.");

        public static Error NameTooLong => Error.Validation(
            code: "Promotion.Name.TooLong",
            description: $"Promotion name cannot exceed {PromotionConstant.Constraints.MaxNameLength} characters.");

        public static Error CodeTooLong => Error.Validation(
            code: "Promotion.Code.TooLong",
            description: $"Promotion code cannot exceed {PromotionConstant.Constraints.MaxCodeLength} characters.");

        public static Error DescriptionTooLong => Error.Validation(
            code: "Promotion.Description.TooLong",
            description: $"Promotion description cannot exceed {PromotionConstant.Constraints.MaxDescriptionLength} characters.");

        public static Error PathTooLong => Error.Validation(
            code: "Promotion.Path.TooLong",
            description: $"Promotion path cannot exceed {PromotionConstant.Constraints.MaxPathLength} characters.");

        public static Error InvalidMatchPolicy => Error.Validation(
            code: "Promotion.MatchPolicy.Invalid",
            description: $"Match policy is invalid. Must be one of: {string.Join(", ", EnumExtensions.GetValues<MatchPolicy>())}");

        public static Error InvalidKind => Error.Validation(
            code: "Promotion.Kind.Invalid",
            description: $"Promotion kind is invalid. Must be one of: {string.Join(", ", EnumExtensions.GetValues<PromotionKind>())}");
        #endregion Validation

        #region Business
        public static Error NotFound(Guid id) => Error.NotFound(
            code: "Promotion.NotFound",
            description: $"Promotion with ID '{id}' was not found.");

        public static Error AlreadyActive => Error.Conflict(
            code: "Promotion.AlreadyActive",
            description: "Promotion is already active.");

        public static Error AlreadyInactive => Error.Conflict(
            code: "Promotion.AlreadyInactive",
            description: "Promotion is already inactive.");

        public static Error ExpiresAtBeforeStartsAt => Error.Validation(
            code: "Promotion.ExpiresAt.InvalidRange",
            description: "Expiration date must be later than the start date.");

        public static Error AlreadyExpired => Error.Conflict(
            code: "Promotion.AlreadyExpired",
            description: "The promotion has already expired.");

        public static Error UsageLimitExceeded => Error.Conflict(
            code: "Promotion.UsageLimitExceeded",
            description: "The promotion's usage limit has been exceeded.");

        public static Error NoActionsDefined => Error.Validation(
            code: "Promotion.NoActionsDefined",
            description: "The promotion has no actions defined.");

        /// <summary>Promotion is no longer active.</summary>
        public static Error Inactive => Error.Validation(
            code: "Promotion.Inactive",
            description: "This promotion is no longer active.");

        /// <summary>Promotion for the given code was not found.</summary>
        public static Error NotFoundByCode => Error.NotFound(
            code: "Promotion.NotFoundByCode",
            description: "Promotion for the given code was not found.");
        #endregion Business
    }
}