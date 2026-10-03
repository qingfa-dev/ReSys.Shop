namespace Module.Catalog.Features.Storefront.Taxonomies.Taxons.Get;

public static partial class GetStoreTaxons
{
    public record Parameters : QueryingParameters
    {
        public int? Depth { get; init; }
    }
}