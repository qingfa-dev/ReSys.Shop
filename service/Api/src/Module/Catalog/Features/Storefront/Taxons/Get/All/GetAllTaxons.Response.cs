using Module.Catalog.Features.Storefront.Taxons.Shared.Models;

namespace Module.Catalog.Features.Storefront.Taxons.Get.All;

public static partial class GetAllTaxons
{
    public record Response : StoreTaxonListItemResponse
    {
        public new int TaxonCount { get; init; }
    }
}
