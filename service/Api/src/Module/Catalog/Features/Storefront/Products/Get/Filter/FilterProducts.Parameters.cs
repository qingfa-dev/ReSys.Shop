namespace Module.Catalog.Features.Storefront.Products.Get.Filter;

public static partial class FilterProducts
{
    public record Parameters : QueryingParameters
    {
        public string? Color { get; init; }
        public string? Size { get; init; }
        public decimal? MinPrice { get; init; }
        public decimal? MaxPrice { get; init; }
        public string? Material { get; init; }
    }
}
