namespace Module.Catalog.Features.Storefront.Products.Get.PagedOrAll;

public static partial class GetStorefrontProducts
{
    public record Parameters : QueryingParameters
    {
        #region Filters
        public Guid[]? OptionValueId { get; init; }
        public Guid[]? TaxonId { get; init; }
        public decimal? MinPrice { get; init; }
        public decimal? MaxPrice { get; init; }
        #endregion
    }
}
