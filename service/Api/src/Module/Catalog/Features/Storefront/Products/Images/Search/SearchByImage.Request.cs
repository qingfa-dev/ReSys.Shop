namespace Module.Catalog.Features.Storefront.Products.Images.Search;

public static partial class SearchByImage
{
    public sealed record Request
    {
        public required IFormFile Image { get; init; }
        public int TopK { get; init; } = 20;
        public string? Model { get; init; }
    }
}
