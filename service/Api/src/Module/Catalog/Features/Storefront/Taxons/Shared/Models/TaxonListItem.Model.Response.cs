using Module.Catalog.Features.Admin.Taxonomies.Taxons.Shared.Models;

namespace Module.Catalog.Features.Storefront.Taxons.Shared.Models;

public record StoreTaxonListItemResponse : TaxonParameters
{
    public Guid Id { get; init; }
    public string Permalink { get; init; } = string.Empty;
    public int Depth { get; init; }
    public int TaxonCount { get; init; }
}
