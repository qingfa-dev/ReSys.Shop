using Module.Catalog.Domain.Taxons;

namespace Module.UnitTests.Catalog.Domain.Taxonomies.Taxons;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "Taxon")]
public class TaxonExtensionsTests
{
    private static Taxon CreateSampleTaxon() => TaxonMethod.Create(
        taxonomyId: Guid.NewGuid(),
        parentId: null,
        name: "Sample",
        presentation: "Sample",
        description: null,
        position: 0,
        slug: "sample",
        metaTitle: null,
        metaDescription: null,
        metaKeywords: null,
        automatic: false,
        rulesMatchPolicy: null,
        sortOrder: null,
        hideFromNav: false,
        imageUrl: null,
        squareImageUrl: null).Value;

    [Theory(DisplayName = "Create: Should return Taxon with correct properties")]
    [InlineData("Shirts", "All Shirts", "Casual and formal shirts", "shirts", 0, false, true)]
    [InlineData("Pants", null, null, "pants", 1, true, false)]
    public void Create_WithValidParameters_ShouldReturnTaxon(
        string name, 
        string? presentation, 
        string? description, 
        string slug, 
        int position, 
        bool hideFromNav,
        bool automatic)
    {
        var taxonomyId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var id = Guid.NewGuid();

        var result = TaxonMethod.Create(
            taxonomyId: taxonomyId,
            parentId: parentId,
            name: name,
            presentation: presentation,
            description: description,
            position: position,
            slug: slug,
            metaTitle: null,
            metaDescription: null,
            metaKeywords: null,
            automatic: automatic,
            rulesMatchPolicy: null,
            sortOrder: null,
            hideFromNav: hideFromNav,
            imageUrl: null,
            squareImageUrl: null,
            id: id);

        var taxon = result.Value;

        result.IsSuccess.Should().BeTrue();
        taxon.Id.Should().Be(id);
        taxon.TaxonomyId.Should().Be(taxonomyId);
        taxon.ParentId.Should().Be(parentId);
        taxon.Name.Should().Be(name.ToLowerInvariant());
        taxon.Presentation.Should().Be(presentation);
        taxon.Description.Should().Be(description);
        taxon.Slug.Should().Be(slug);
        taxon.Position.Should().Be(position);
        taxon.HideFromNav.Should().Be(hideFromNav);
        taxon.Automatic.Should().Be(automatic);

        if (automatic)
        {
            taxon.MarkedForRegenerateTaxonProducts.Should().BeTrue();
        }
    }

    [Fact(DisplayName = "Update: Should update properties")]
    public void Update_WithNewValues_ShouldUpdateCorrectly()
    {
        var taxon = CreateSampleTaxon();
        var newName = "Updated Name";

        var result = taxon.Update(
            parentId: null,
            name: newName,
            presentation: null,
            description: null,
            position: null,
            slug: null,
            metaTitle: null,
            metaDescription: null,
            metaKeywords: null,
            automatic: true,
            rulesMatchPolicy: null,
            sortOrder: null,
            hideFromNav: null,
            imageUrl: null,
            squareImageUrl: null);

        result.IsSuccess.Should().BeTrue();
        taxon.Name.Should().Be("updated name");
        taxon.Automatic.Should().BeTrue();
        taxon.MarkedForRegenerateTaxonProducts.Should().BeTrue();
    }

    [Fact(DisplayName = "Create: Should honor explicit slug and not regenerate from name")]
    public void Create_WithExplicitDistinctSlug_ShouldHonorSlugAndNotRegenerateFromName()
    {
        var taxonomyId = Guid.NewGuid();
        var id = Guid.NewGuid();

        var result = TaxonMethod.Create(
            taxonomyId: taxonomyId,
            parentId: null,
            name: "Foo Bar",
            presentation: "Foo Bar",
            description: null,
            position: 0,
            slug: "custom-disambiguated-slug",
            metaTitle: null,
            metaDescription: null,
            metaKeywords: null,
            automatic: false,
            rulesMatchPolicy: null,
            sortOrder: null,
            hideFromNav: false,
            imageUrl: null,
            squareImageUrl: null,
            id: id);

        var taxon = result.Value;

        result.IsSuccess.Should().BeTrue();
        taxon.Slug.Should().Be("custom-disambiguated-slug");
        taxon.Slug.Should().NotBe("foo-bar");
    }

    [Fact(DisplayName = "Create: Should slugify from name when slug is empty")]
    public void Create_WithEmptySlug_ShouldSlugifyFromName()
    {
        var taxonomyId = Guid.NewGuid();

        var result = TaxonMethod.Create(
            taxonomyId: taxonomyId,
            parentId: null,
            name: "Foo Bar",
            presentation: "Foo Bar",
            description: null,
            position: 0,
            slug: "",
            metaTitle: null,
            metaDescription: null,
            metaKeywords: null,
            automatic: false,
            rulesMatchPolicy: null,
            sortOrder: null,
            hideFromNav: false,
            imageUrl: null,
            squareImageUrl: null);

        var taxon = result.Value;

        result.IsSuccess.Should().BeTrue();
        taxon.Slug.Should().Be("foo-bar");
    }

    [Fact(DisplayName = "Update: With explicit slug should preserve explicit slug over name-based slugification")]
    public void Update_WithExplicitSlug_ShouldPreserveExplicitSlug()
    {
        var taxon = CreateSampleTaxon();

        taxon.Update(
            parentId: null,
            name: "Different Name",
            presentation: null,
            description: null,
            position: null,
            slug: "preserved-explicit-slug",
            metaTitle: null,
            metaDescription: null,
            metaKeywords: null,
            automatic: null,
            rulesMatchPolicy: null,
            sortOrder: null,
            hideFromNav: null,
            imageUrl: null,
            squareImageUrl: null);

        taxon.Slug.Should().Be("preserved-explicit-slug");
        taxon.Slug.Should().NotBe("different-name");
    }

    [Fact(DisplayName = "Update: With parent change should update parent")]
    public void Update_WithParentChange_ShouldUpdateParent()
    {
        var taxon = CreateSampleTaxon();
        var newParentId = Guid.NewGuid();

        var result = taxon.Update(
            parentId: newParentId,
            name: null,
            presentation: null,
            description: null,
            position: 10,
            slug: null,
            metaTitle: null,
            metaDescription: null,
            metaKeywords: null,
            automatic: null,
            rulesMatchPolicy: null,
            sortOrder: null,
            hideFromNav: null,
            imageUrl: null,
            squareImageUrl: null);

        result.IsSuccess.Should().BeTrue();
        taxon.ParentId.Should().Be(newParentId);
        taxon.Position.Should().Be(10);
    }

    [Fact(DisplayName = "Move: To new parent and position should update correctly")]
    public void Move_ToNewParentAndPosition_ShouldUpdateCorrectly()
    {
        var taxon = CreateSampleTaxon();
        var newParentId = Guid.NewGuid();
        var newPosition = 5;

        var result = taxon.Move(newParentId, newPosition);

        result.IsSuccess.Should().BeTrue();
        taxon.ParentId.Should().Be(newParentId);
        taxon.Position.Should().Be(newPosition);
    }

    [Fact(DisplayName = "Move: Same parent and position should return Ok")]
    public void Move_ToSameParentAndPosition_ShouldReturnOk()
    {
        var taxon = CreateSampleTaxon();

        var result = taxon.Move(taxon.ParentId, taxon.Position);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "Delete: Should soft delete")]
    public void Delete_ShouldRaiseEvent()
    {
        var taxon = CreateSampleTaxon();

        var result = taxon.Delete();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "UpdatePermalink: Should correctly handle primary root vs auxiliary root")]
    public void UpdatePermalink_ShouldDistinguishRoots()
    {
        var primaryRoot = CreateSampleTaxon();
        primaryRoot.Slug = "categories";
        
        var auxRoot = CreateSampleTaxon();
        auxRoot.Slug = "other";

        primaryRoot.UpdatePermalink("Categories");
        auxRoot.UpdatePermalink("Categories");

        primaryRoot.Permalink.Should().Be("categories"); 
        auxRoot.Permalink.Should().Be("categories/other");
    }

    [Fact(DisplayName = "Restore: Should set IsDeleted false")]
    public void Restore_ShouldSetIsDeletedFalse()
    {
        var taxon = CreateSampleTaxon();
        taxon.Delete();

        var result = taxon.Restore();

        result.IsSuccess.Should().BeTrue();
        taxon.IsDeleted.Should().BeFalse();
    }

    [Fact(DisplayName = "UpdatePrettyName: With parent should build path")]
    public void UpdatePrettyName_WithParent_ShouldBuildPath()
    {
        var parent = CreateSampleTaxon();
        parent.Presentation = "Parent";
        parent.UpdatePrettyName("Taxonomy");
        var child = CreateSampleTaxon();
        child.Parent = parent;
        child.Presentation = "Child";

        child.UpdatePrettyName("Taxonomy");

        child.PrettyName.Should().Be("Parent -> Child");
    }

    [Fact(DisplayName = "UpdatePrettyName: Without parent should use own presentation")]
    public void UpdatePrettyName_WithoutParent_ShouldUseOwnPresentation()
    {
        var taxon = CreateSampleTaxon();
        taxon.Parent = null;
        taxon.Presentation = "Root";

        taxon.UpdatePrettyName("Taxonomy");

        taxon.PrettyName.Should().Be("Root");
    }
}
