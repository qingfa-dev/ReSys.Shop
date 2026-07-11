using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Storefront.Taxonomies.Shared.Mappings;
using Module.Catalog.Features.Storefront.Taxonomies.Shared.Models;

namespace Module.UnitTests.Catalog.Features.Storefront.Taxonomies.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonomyStoreMapping")]
public class TaxonomyStoreMappingTests
{
    [Fact(DisplayName = "MapToStoreTree: Should map Taxonomy with nested taxons")]
    public void MapToStoreTree_ShouldMapNestedTree()
    {
        var taxonomy = CreateTaxonomy();
        var root = taxonomy.Taxons.Single(t => t.ParentId == null);
        var children = taxonomy.Taxons.Where(t => t.ParentId == root.Id).ToList();

        var response = taxonomy.MapToStoreTree<StoreTaxonomyTreeResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(taxonomy.Id);
        response.Name.Should().Be(taxonomy.Name);
        response.Presentation.Should().Be(taxonomy.Presentation);
        response.Nodes.Should().HaveCount(1);

        var rootNode = response.Nodes[0];
        AssertTaxonTreeNode(rootNode, root);
        rootNode.Children.Should().HaveCount(2);
        AssertTaxonTreeNode(rootNode.Children[0], children[0]);
        AssertTaxonTreeNode(rootNode.Children[1], children[1]);
    }

    [Fact(DisplayName = "MapToStoreTree: Should exclude hidden or deleted taxons")]
    public void MapToStoreTree_ShouldExcludeHiddenAndDeletedTaxons()
    {
        var taxonomy = CreateTaxonomy();
        var hiddenResult = TaxonMethod.Create(
            taxonomy.Id, null, "Hidden", null, null, 0, "hidden", null, null, null,
            false, null, null, hideFromNav: true, null, null);
        hiddenResult.IsSuccess.Should().BeTrue();
        taxonomy.Taxons.Add(hiddenResult.Value);

        var response = taxonomy.MapToStoreTree<StoreTaxonomyTreeResponse>();

        response.Nodes.Should().HaveCount(1);
        response.Nodes[0].Children.Should().HaveCount(2);
    }

    private static Taxonomy CreateTaxonomy()
    {
        var taxResult = TaxonomyExtensions.Create("Categories", "Categories", position: 1);
        taxResult.IsSuccess.Should().BeTrue();
        var taxonomy = taxResult.Value;

        var rootResult = TaxonMethod.Create(
            taxonomy.Id, null, "Root", "Root", null, 0, "root", null, null, null,
            false, null, null, hideFromNav: false, null, null);
        rootResult.IsSuccess.Should().BeTrue();
        var root = rootResult.Value;
        taxonomy.Taxons.Add(root);

        var child1Result = TaxonMethod.Create(
            taxonomy.Id, root.Id, "Child 1", "Child 1", null, 1, "child-1", null, null, null,
            false, null, null, hideFromNav: false, null, null);
        child1Result.IsSuccess.Should().BeTrue();
        taxonomy.Taxons.Add(child1Result.Value);

        var child2Result = TaxonMethod.Create(
            taxonomy.Id, root.Id, "Child 2", "Child 2", null, 2, "child-2", null, null, null,
            false, null, null, hideFromNav: false, null, null);
        child2Result.IsSuccess.Should().BeTrue();
        taxonomy.Taxons.Add(child2Result.Value);

        return taxonomy;
    }

    private static void AssertTaxonTreeNode(TaxonTreeNode node, Taxon taxon)
    {
        node.Id.Should().Be(taxon.Id);
        node.Name.Should().Be(taxon.Name);
        node.Presentation.Should().Be(taxon.Presentation);
        node.Depth.Should().Be(taxon.Depth);
    }
}
