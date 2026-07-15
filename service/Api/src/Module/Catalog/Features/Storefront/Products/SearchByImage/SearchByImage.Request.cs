namespace Module.Catalog.Features.Storefront.Products.SearchByImage;

public static partial class SearchByImage
{
    public record Request
    {
        public required IFormFile Image { get; init; }
    }
}
