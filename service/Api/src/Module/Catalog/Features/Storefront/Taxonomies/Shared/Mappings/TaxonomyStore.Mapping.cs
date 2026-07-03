using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Storefront.Taxonomies.Shared.Models;

namespace Module.Catalog.Features.Storefront.Taxonomies.Shared.Mappings;

public static class TaxonomyStoreMapping
{
    public static T MapToStoreTree<T>(this Taxonomy entity) where T : StoreTaxonomyTreeResponse, new()
    {
        var sortedTaxons = entity.Taxons
            .Where(t => !t.IsDeleted && !t.HideFromNav)
            .OrderBy(t => t.Lft)
            .ToList();

        var rootNodes = BuildTree(sortedTaxons, null);

        return new T
        {
            Id = entity.Id,
            Name = entity.Name,
            Presentation = entity.Presentation,
            Nodes = rootNodes,
        };
    }

    private static List<TaxonTreeNode> BuildTree(List<Taxon> sortedTaxons, Guid? parentId)
    {
        return sortedTaxons
            .Where(t => t.ParentId == parentId)
            .Select(t => new TaxonTreeNode
            {
                Id = t.Id,
                Name = t.Name,
                Presentation = t.Presentation,
                Permalink = t.Permalink,
                Depth = t.Depth,
                Children = BuildTree(sortedTaxons, t.Id),
            })
            .ToList();
    }
}
