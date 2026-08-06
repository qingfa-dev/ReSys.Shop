using Module.Catalog.Features.Admin.Taxons.Shared.Models;

namespace Module.Catalog.Features.Storefront.Classifications.Shared.Models;

public record TaxonBreadcrumbItem(Guid Id, string Name, string Permalink);

public record StoreTaxonListItemResponse : TaxonListItemResponse
{
    public List<TaxonBreadcrumbItem> Breadcrumb { get; init; } = [];
}