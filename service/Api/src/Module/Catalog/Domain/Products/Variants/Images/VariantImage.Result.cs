namespace Module.Catalog.Domain.Products.Variants.Images;

public static class VariantImageResult
{
    public static class Success
    {
        /// <summary>Returns a success message for image creation.</summary>
        public static string Created(Guid id) => $"Variant image with ID '{id}' was successfully created.";
        /// <summary>Returns a success message for image update.</summary>
        public static string Updated(Guid id) => $"Variant image with ID '{id}' was successfully updated.";
        /// <summary>Returns a success message for image deletion.</summary>
        public static string Deleted(Guid id) => $"Variant image with ID '{id}' was successfully deleted.";
    }

    public static class Failure
    {
        #region Validation
        /// <summary>Variant image was not found by ID.</summary>
        public static Error ById(Guid id) => Error.NotFound(
            code: "VariantImage.NotFound",
            message: $"Variant image with ID '{id}' was not found.");

        /// <summary>Variant image was not found.</summary>
        public static Error NotFound => Error.NotFound(
            code: "VariantImage.NotFound",
            message: "Variant image not found.");

        /// <summary>Image URL is required.</summary>
        public static Error UrlRequired => Error.Validation(
            code: "VariantImage.UrlRequired",
            message: "Image URL is required.");

        /// <summary>Image URL exceeds the maximum length.</summary>
        public static Error UrlTooLong => Error.Validation(
            code: "VariantImage.UrlTooLong",
            message: $"Image URL must not exceed {VariantImageConstant.Constraints.UrlMaxLength} characters.");

        /// <summary>Content type exceeds the maximum length.</summary>
        public static Error ContentTypeTooLong => Error.Validation(
            code: "VariantImage.ContentType.TooLong",
            message: $"Content type cannot exceed {VariantImageConstant.Constraints.ContentTypeMaxLength} characters.");

        /// <summary>File name exceeds the maximum length.</summary>
        public static Error FileNameTooLong => Error.Validation(
            code: "VariantImage.FileName.TooLong",
            message: $"File name cannot exceed {VariantImageConstant.Constraints.FileNameMaxLength} characters.");

        /// <summary>Alt text exceeds the maximum length.</summary>
        public static Error AltTooLong => Error.Validation(
            code: "VariantImage.Alt.TooLong",
            message: $"Alt text cannot exceed {VariantImageConstant.Constraints.AltMaxLength} characters.");

        /// <summary>Position must be greater than or equal to the minimum value.</summary>
        public static Error InvalidPosition => Error.Validation(
            code: "VariantImage.Position.Invalid",
            message: $"Position must be greater than or equal to {VariantImageConstant.Constraints.MinPosition}.");

        /// <summary>File size must be greater than zero.</summary>
        public static Error InvalidFileSize => Error.Validation(
            code: "VariantImage.FileSize.Invalid",
            message: "File size must be greater than zero.");

        /// <summary>Dimensions must be within the allowed range.</summary>
        public static Error InvalidDimension => Error.Validation(
            code: "VariantImage.Dimension.Invalid",
            message: $"Dimensions must be between {VariantImageConstant.Constraints.MinDimension} and {VariantImageConstant.Constraints.MaxDimension}.");

        /// <summary>Dimensions unit is not in the allowed list.</summary>
        public static Error InvalidDimensionsUnit => Error.Validation(
            code: "VariantImage.DimensionsUnit.Invalid",
            message: $"Dimensions unit must be one of: {string.Join(", ", VariantImageConstant.Constraints.Dimensions.AllowedUnits)}.");

        /// <summary>Image type is invalid.</summary>
        public static Error InvalidType => Error.Validation(
            code: "VariantImage.Type.Invalid",
            message: $"Image type is invalid. Must be one of: {string.Join(", ", EnumExtensions.GetValues<VariantImageType>())}");

        /// <summary>An image file is required.</summary>
        public static Error FileRequired => Error.Validation(
            code: "VariantImage.File.Required",
            message: "An image file is required.");

        /// <summary>File must not be empty.</summary>
        public static Error FileEmpty => Error.Validation(
            code: "VariantImage.File.Empty",
            message: "File must not be empty.");

        /// <summary>File size exceeds the maximum allowed.</summary>
        public static Error FileTooLarge => Error.Validation(
            code: "VariantImage.File.TooLarge",
            message: $"File size must not exceed {VariantImageConstant.Constraints.Upload.MaxFileSizeBytes} bytes.");

        /// <summary>Content type is not allowed.</summary>
        public static readonly Error InvalidContentType = Error.Validation(
            code: "VariantImage.ContentType.Invalid",
            message: $"Content type is not allowed. Must be one of: {string.Join(", ", VariantImageConstant.Constraints.Upload.AllowedContentTypes)}.");

        /// <summary>Returns a content type invalidation message.</summary>
        public static string InvalidContentTypeMessage(string contentType) =>
            $"Content type '{contentType}' is not allowed. Must be one of: {string.Join(", ", VariantImageConstant.Constraints.Upload.AllowedContentTypes)}.";
        #endregion
    }
}