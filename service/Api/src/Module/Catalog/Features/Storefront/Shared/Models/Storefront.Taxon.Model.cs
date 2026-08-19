using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.Catalog.Features.Storefront.Shared.Models;

public record TaxonBreadcrumbItem(Guid Id, string Name, string Permalink);

public record StoreTaxonListItemResponse : TaxonListItemResponse
{
    public List<TaxonBreadcrumbItem> Breadcrumb { get; init; } = [];
}
