using Module.Catalog.Features.Storefront.Classifications.Shared.Models;

namespace Module.Catalog.Features.Storefront.Classifications.Taxons.GetByPermalink;

public static partial class GetTaxonByPermalink
{
    public record Response : StoreTaxonListItemResponse
    {
        public List<TaxonBreadcrumbItem> Children { get; init; } = [];
    }
}