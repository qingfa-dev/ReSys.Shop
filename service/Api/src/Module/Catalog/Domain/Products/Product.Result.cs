namespace Module.Catalog.Domain.Products;

public static class ProductResult
{
    public static class Success
    {
        /// <summary>Returns a success message for product creation.</summary>
        public static string Created(Guid id) => $"Product with ID '{id}' was successfully created.";
        /// <summary>Returns a success message for product update.</summary>
        public static string Updated(Guid id) => $"Product with ID '{id}' was successfully updated.";
        /// <summary>Returns a success message for product deletion.</summary>
        public static string Deleted(Guid id) => $"Product with ID '{id}' was successfully deleted.";
        /// <summary>Product was successfully activated.</summary>
        public static string Activated => "Product was successfully activated.";
        /// <summary>Product was successfully archived.</summary>
        public static string Archived => "Product was successfully archived.";
        /// <summary>Product was successfully set to draft.</summary>
        public static string Drafted => "Product was successfully set to draft.";
        /// <summary>Product was successfully discontinued.</summary>
        public static string Discontinued => "Product was successfully discontinued.";
        /// <summary>Fashion fields were successfully updated.</summary>
        public static string FashionFieldsUpdated => "Fashion fields were successfully updated.";
    }

    public static class Errors
    {
        #region Validation
        /// <summary>Product name is required.</summary>
        public static Error NameRequired => Error.Validation(
            code: "Product.Name.Required",
            message: "Product name is required.");

        /// <summary>Product name exceeds the maximum length.</summary>
        public static Error NameTooLong => Error.Validation(
            code: "Product.Name.TooLong",
            message: $"Product name cannot exceed {ProductConstant.Constraints.MaxNameLength} characters.");

        /// <summary>Product slug is required.</summary>
        public static Error SlugRequired => Error.Validation(
            code: "Product.Slug.Required",
            message: "Product slug is required.");

        /// <summary>Product slug exceeds the maximum length.</summary>
        public static Error SlugTooLong => Error.Validation(
            code: "Product.Slug.TooLong",
            message: $"Product slug cannot exceed {ProductConstant.Constraints.MaxSlugLength} characters.");

        /// <summary>Product message exceeds the maximum length.</summary>
        public static Error DescriptionTooLong => Error.Validation(
            code: "Product.Description.TooLong",
            message: $"Product message cannot exceed {ProductConstant.Constraints.MaxDescriptionLength} characters.");

        /// <summary>Product meta title exceeds the maximum length.</summary>
        public static Error MetaTitleTooLong => Error.Validation(
            code: "Product.MetaTitle.TooLong",
            message: $"Product meta title cannot exceed {ProductConstant.Constraints.MaxMetaTitleLength} characters.");

        /// <summary>Product meta message exceeds the maximum length.</summary>
        public static Error MetaDescriptionTooLong => Error.Validation(
            code: "Product.MetaDescription.TooLong",
            message: $"Product meta message cannot exceed {ProductConstant.Constraints.MaxMetaDescriptionLength} characters.");

        /// <summary>Product meta keywords exceeds the maximum length.</summary>
        public static Error MetaKeywordsTooLong => Error.Validation(
            code: "Product.MetaKeywords.TooLong",
            message: $"Product meta keywords cannot exceed {ProductConstant.Constraints.MaxMetaKeywordsLength} characters.");

        /// <summary>Product status is not a valid value.</summary>
        public static Error InvalidStatus => Error.Validation(
            code: "Product.Status.Invalid",
            message: $"Product status is invalid. Must be one of: {string.Join(", ", EnumExtensions.GetValues<ProductStatus>())}");
        #endregion

        #region Business
        /// <summary>Product was not found by ID.</summary>
        public static Error NotFound(Guid id) => Error.NotFound(
            code: "Product.NotFound",
            message: $"Product with ID '{id}' was not found.");

        /// <summary>Product was not found by slug.</summary>
        public static Error NotFoundBySlug(string slug) => Error.NotFound(
            code: "Product.NotFound.Slug",
            message: $"Product with slug '{slug}' was not found.");

        /// <summary>Duplicate slug detected.</summary>
        public static Error DuplicateSlug => Error.Conflict(
            code: "Product.DuplicateSlug",
            message: "A product with the same slug already exists.");

        /// <summary>Product is already deleted.</summary>
        public static Error AlreadyDeleted => Error.Conflict(
            code: "Product.AlreadyDeleted",
            message: "Product is already deleted.");

        /// <summary>Product is already active.</summary>
        public static Error AlreadyActive => Error.Conflict(
            code: "Product.AlreadyActive",
            message: "Product is already active.");

        /// <summary>Product is already archived.</summary>
        public static Error AlreadyArchived => Error.Conflict(
            code: "Product.AlreadyArchived",
            message: "Product is already archived.");

        /// <summary>Product is already in draft status.</summary>
        public static Error AlreadyDraft => Error.Conflict(
            code: "Product.AlreadyDraft",
            message: "Product is already in draft status.");

        /// <summary>Product is already discontinued.</summary>
        public static Error AlreadyDiscontinued => Error.Conflict(
            code: "Product.AlreadyDiscontinued",
            message: "Product is already discontinued.");

        /// <summary>Discontinue date must be later than the available-on date.</summary>
        public static Error DiscontinueOnBeforeAvailableOn => Error.Validation(
            code: "Product.DiscontinueOn.InvalidRange",
            message: "Discontinue date must be later than the available-on date.");

        /// <summary>Make-active date cannot be in the past.</summary>
        public static Error MakeActiveAtInPast => Error.Validation(
            code: "Product.MakeActiveAt.InPast",
            message: "Make-active date cannot be in the past.");

        /// <summary>Invalid status transition attempted.</summary>
        public static Error InvalidStatusTransition => Error.Validation(
            code: "Product.StatusTransition.Invalid",
            message: "Invalid status transition.");

        /// <summary>Cannot activate an archived product.</summary>
        public static Error CannotActivateArchivedProduct => Error.Validation(
            code: "Product.CannotActivateArchivedProduct",
            message: "Cannot activate an archived product.");
        #endregion
    }
}