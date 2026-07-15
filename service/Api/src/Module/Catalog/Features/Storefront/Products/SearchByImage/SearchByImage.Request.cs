namespace Module.Catalog.Features.Storefront.Products.SearchByImage;

public static partial class SearchByImage
{
    // EXCEPTION: file upload request — IFormFile has no Parameters base
    public record Request
    {
        public required IFormFile Image { get; init; }
    }
}
