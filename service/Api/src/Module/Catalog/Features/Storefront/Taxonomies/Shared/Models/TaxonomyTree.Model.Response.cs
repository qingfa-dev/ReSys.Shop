using Module.Catalog.Features.Admin.Taxonomies.Shared.Models;

namespace Module.Catalog.Features.Storefront.Taxonomies.Shared.Models;

public record StoreTaxonomyTreeResponse : TaxonomyParameters
{
    public Guid Id { get; init; }
    public List<TaxonTreeNode> Nodes { get; init; } = [];
}

public class TaxonTreeNode
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Presentation { get; init; }
    public string Permalink { get; init; } = string.Empty;
    public int Depth { get; init; }
    public bool HasChildren => Children.Count > 0;
    public List<TaxonTreeNode> Children { get; init; } = [];
}
