using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Shared.Mappings;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Shared.Models;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Taxonomies")]
[Trait("Concern", "Mapping")]
public class TaxonMappingTests
{
    private readonly Guid _taxonomyId = Guid.NewGuid();

    [Fact(DisplayName = "MapToListItemResponse: Should map Taxon entity to TaxonListItemResponse")]
    public void MapToListItemResponse_ShouldMapEntityToResponse()
    {
        // Arrange
        var taxon = TaxonMethod.Create(
            taxonomyId: _taxonomyId,
            parentId: null,
            name: "Category",
            presentation: "Category Display",
            description: "Description",
            position: 1,
            slug: "category",
            metaTitle: "Meta Title",
            metaDescription: "Meta Description",
            metaKeywords: "Keywords",
            automatic: true,
            rulesMatchPolicy: TaxonMatchPolicy.Any,
            sortOrder: TaxonSortOrder.Newest,
            hideFromNav: true,
            imageUrl: "image.png",
            squareImageUrl: "square.png"
        ).Value;

        taxon.CreatedAtUtc = DateTimeOffset.UtcNow;
        taxon.ModifiedAtUtc = DateTimeOffset.UtcNow;
        taxon.Permalink = "permalink";
        taxon.PrettyName = "Pretty Name";
        taxon.Lft = 1;
        taxon.Rgt = 2;
        taxon.Depth = 0;

        // Act
        var response = taxon.MapToListItem<TaxonListItemResponse>();

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(taxon.Id);
        response.Name.Should().Be(taxon.Name);
        response.Presentation.Should().Be(taxon.Presentation);
        response.Description.Should().Be(taxon.Description);
        response.Position.Should().Be(taxon.Position);
        response.Slug.Should().Be(taxon.Slug);
        response.MetaTitle.Should().Be(taxon.MetaTitle);
        response.MetaDescription.Should().Be(taxon.MetaDescription);
        response.MetaKeywords.Should().Be(taxon.MetaKeywords);
        response.ImageUrl.Should().Be(taxon.ImageUrl);
        response.SquareImageUrl.Should().Be(taxon.SquareImageUrl);
        response.Automatic.Should().Be(taxon.Automatic);
        response.RulesMatchPolicy.Should().Be(taxon.RulesMatchPolicy);
        response.SortOrder.Should().Be(taxon.SortOrder);
        response.HideFromNav.Should().Be(taxon.HideFromNav);
        response.Lft.Should().Be(taxon.Lft);
        response.Rgt.Should().Be(taxon.Rgt);
        response.Depth.Should().Be(taxon.Depth);
        response.Permalink.Should().Be(taxon.Permalink);
        response.PrettyName.Should().Be(taxon.PrettyName);
        response.CreatedAtUtc.Should().Be(taxon.CreatedAtUtc);
        response.ModifiedAtUtc.Should().Be(taxon.ModifiedAtUtc);
    }

    [Fact(DisplayName = "MapToDetailResponse: Should map Taxon entity to TaxonDetailItemResponse")]
    public void MapToDetailResponse_ShouldMapEntityToResponse()
    {
        // Arrange
        var parent = TaxonMethod.Create(_taxonomyId, null, "Parent", null, null, 0, "parent", null, null, null, false, null, null, false, null, null).Value;
        var taxon = TaxonMethod.Create(_taxonomyId, parent.Id, "Child", null, null, 1, "child", null, null, null, false, null, null, false, null, null).Value;
        taxon.Parent = parent;

        // Act
        var response = taxon.MapToDetail<TaxonDetailResponse>();

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(taxon.Id);
        response.Name.Should().Be(taxon.Name);
        response.ParentId.Should().Be(parent.Id);
        response.ParentName.Should().Be(parent.Name);
    }

    [Fact(DisplayName = "MapToTreeItem: Should map Taxon entity to TaxonTreeItem with children")]
    public void MapToTreeItem_ShouldMapEntityToResponseWithChildren()
    {
        // Arrange
        var root = TaxonMethod.Create(_taxonomyId, null, "Root", null, null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var child1 = TaxonMethod.Create(_taxonomyId, root.Id, "Child 1", null, null, 1, "child-1", null, null, null, false, null, null, false, null, null).Value;
        var child2 = TaxonMethod.Create(_taxonomyId, root.Id, "Child 2", null, null, 2, "child-2", null, null, null, false, null, null, false, null, null).Value;

        root.Children.Add(child1);
        root.Children.Add(child2);

        // Act
        var response = root.MapToTreeItem<TaxonTreeItem>();

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(root.Id);
        response.Children.Should().HaveCount(2);
        response.Children.Should().Contain(c => c.Id == child1.Id);
        response.Children.Should().Contain(c => c.Id == child2.Id);
    }

    [Fact(DisplayName = "MapToDomain: Should map TaxonRequest to Taxon entity")]
    public void MapToDomain_ShouldMapRequestToEntity()
    {
        var request = new TaxonRequest
        {
            Name = "Test Category",
            Presentation = "Test Display",
            Slug = "test-category",
            Description = "A test description",
            Position = 5,
            Automatic = true,
            HideFromNav = false
        };

        var result = request.MapToDomain(_taxonomyId);
        var entity = result.Value;

        result.IsSuccess.Should().BeTrue();
        entity.Should().NotBeNull();
        entity.Name.Should().Be("test category");
        entity.TaxonomyId.Should().Be(_taxonomyId);
        entity.Automatic.Should().BeTrue();
    }

    [Fact(DisplayName = "MapToDomain (Update): Should update existing Taxon entity from request")]
    public void MapToDomain_Update_ShouldUpdateEntity()
    {
        var entity = TaxonMethod.Create(_taxonomyId, null, "Old Name", "Old Display", "Old Desc", 0, "old-name", null, null, null, false, null, null, false, null, null).Value;

        var request = new TaxonRequest
        {
            Name = "New Name",
            Slug = "new-name",
            Position = 10
        };

        var result = request.MapToDomain(entity);

        result.IsSuccess.Should().BeTrue();
        entity.Name.Should().Be("new name");
        entity.Position.Should().Be(10);
    }

    [Fact(DisplayName = "MapToTreeItem: Should set expanded and active path flags")]
    public void MapToTreeItem_ShouldSetFlags()
    {
        var root = TaxonMethod.Create(_taxonomyId, null, "Root", null, null, 0, "root", null, null, null, false, null, null, false, null, null).Value;

        root.Lft = 1; root.Rgt = 2; root.Depth = 0;

        var response = root.MapToTreeItem<TaxonTreeItem>();

        response.IsExpanded.Should().BeFalse();
        response.IsInActivePath.Should().BeFalse();
    }
}
