namespace Module.Catalog.Features.Storefront.Products.Get.List;

public static partial class ListProducts
{
    public record Parameters : QueryingParameters
    {
        public string? OptionValue { get; init; }
        public string? OptionType { get; init; }
        public string? Taxon { get; init; }
        public decimal? MinPrice { get; init; }
        public decimal? MaxPrice { get; init; }
        public new string? Search { get; init; }
    }
}
