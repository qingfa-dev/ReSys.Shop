namespace Module.Catalog.Features.Storefront.Products.Get.Search;

public static partial class SearchProducts
{
    public record Parameters : QueryingParameters
    {
        public string? Q { get; init; }
    }
}
