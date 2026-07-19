namespace Module.Catalog.Features.Storefront.Products.SearchByImage;

public static class SearchByImageResult
{
    public static class Errors
    {
        /// <summary>Uploaded image exceeds the 10 MB size limit.</summary>
        public static Error FileTooLarge => Error.Validation(
            code: "SearchByImage.FileTooLarge",
            message: "Image file must not exceed 10 MB.");

        /// <summary>Uploaded file is not a valid image type.</summary>
        public static Error InvalidContentType => Error.Validation(
            code: "SearchByImage.InvalidContentType",
            message: "File must be an image.");
    }
}
