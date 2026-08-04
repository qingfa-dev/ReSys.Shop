namespace Module.Catalog.Features.Storefront.Products.Get.List;

public static partial class GetStorefrontProductPagedOrAll
{
    public record Parameters : QueryingParameters
    {
        public Guid[]? OptionValueId { get; init; }
        public Guid[]? TaxonId { get; init; }
        public decimal? MinPrice { get; init; }
        public decimal? MaxPrice { get; init; }
        public bool IncludeFacets { get; init; }
        public new string? Search { get; init; }
    }
}
