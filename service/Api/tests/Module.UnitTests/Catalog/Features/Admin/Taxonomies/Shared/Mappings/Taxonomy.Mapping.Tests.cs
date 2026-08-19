using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxons;
using Module.Catalog.Features.Admin.Shared.Mappings;
using Module.Catalog.Features.Admin.Shared.Models;

using Shared.Application.Domain.Concerns.Parameterizable;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Taxonomies")]
[Trait("Concern", "Mapping")]
public class TaxonomyMappingTests
{
    [Fact(DisplayName = "MapToDomain: Should map TaxonomyRequest to Taxonomy entity")]
    public void MapToDomain_ShouldMapRequestToEntity()
    {
        var request = new TaxonomyRequest { Name = "Category", Presentation = "Category Display", Position = 1 };

        var result = request.MapToDomain();
        var entity = result.Value;

        result.IsSuccess.Should().BeTrue();
        entity.Should().NotBeNull();
        entity.Name.Should().Be(ParameterizableBehavior.Normalize(request.Name));
        entity.Presentation.Should().Be(request.Presentation);
        entity.Position.Should().Be(request.Position);
    }

    [Fact(DisplayName = "MapToDomain (Update): Should update existing Taxonomy entity from request")]
    public void MapToDomain_Update_ShouldUpdateEntity()
    {
        var request = new TaxonomyRequest { Name = "New Category", Presentation = "New Display", Position = 10 };

        var entity = TaxonomyMethod.Create("Old Name", "Old Presentation", 0).Value;

        var result = request.MapToDomain(entity);

        result.IsSuccess.Should().BeTrue();
        entity.Name.Should().Be(ParameterizableBehavior.Normalize(request.Name));
        entity.Presentation.Should().Be(request.Presentation);
        entity.Position.Should().Be(request.Position);
    }

    [Fact(DisplayName = "MapToDetail: Should map Taxonomy entity to TaxonomyDetailResponse")]
    public void MapToDetail_ShouldMapEntityToResponse()
    {
        var entity = TaxonomyMethod.Create("Category", "Category Display", 1).Value;

        var result = entity.MapToDetail<TaxonomyDetailResponse>();

        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
        result.Name.Should().Be(entity.Name);
        result.Presentation.Should().Be(entity.Presentation);
        result.Position.Should().Be(entity.Position);
        result.CreatedAtUtc.Should().Be(entity.CreatedAtUtc);
        result.ModifiedAtUtc.Should().Be(entity.ModifiedAtUtc);
    }

    [Fact(DisplayName = "MapToListItem: Should map Taxonomy entity to TaxonomyListItemResponse")]
    public void MapToListItem_ShouldMapEntityToResponse()
    {
        var entity = TaxonomyMethod.Create("Category", "Category Display", 5).Value;

        var result = entity.MapToListItem<TaxonomyListItemResponse>();

        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
        result.Name.Should().Be(entity.Name);
        result.Presentation.Should().Be(entity.Presentation);
        result.Position.Should().Be(entity.Position);
        result.TaxonsCount.Should().Be(entity.Taxons.Count);
        result.CreatedAtUtc.Should().Be(entity.CreatedAtUtc);
        result.ModifiedAtUtc.Should().Be(entity.ModifiedAtUtc);
    }

    [Fact(DisplayName = "MapToDomain: Should handle empty presentation")]
    public void MapToDomain_ShouldHandleEmptyPresentation()
    {
        var request = new TaxonomyRequest { Name = "EmptyPresentation", Presentation = "", Position = 0 };

        var result = request.MapToDomain();
        var entity = result.Value;

        result.IsSuccess.Should().BeTrue();
        entity.Name.Should().Be("emptypresentation");
        entity.Presentation.Should().Be("");
        entity.Position.Should().Be(0);
    }

    [Fact(DisplayName = "MapToDomain: Should handle negative position")]
    public void MapToDomain_ShouldHandleNegativePosition()
    {
        var request = new TaxonomyRequest { Name = "Negative", Presentation = "Negative Position", Position = -1 };

        var result = request.MapToDomain();
        var entity = result.Value;

        result.IsSuccess.Should().BeTrue();
        entity.Position.Should().Be(-1);
    }

    [Fact(DisplayName = "MapToListItem: Should include taxons count with children")]
    public void MapToListItem_ShouldIncludeTaxonsCount_WhenEntityHasChildren()
    {
        var taxonomyId = Guid.NewGuid();
        var entity = TaxonomyMethod.Create("Category", "Display", 5).Value;
        typeof(Taxonomy).GetProperty("Id")!.SetValue(entity, taxonomyId);

        var child1 = TaxonMethod.Create(taxonomyId, null, "Child1", "Child1", null, 0, "child1", null, null, null, false, null, null, false, null, null).Value;
        var child2 = TaxonMethod.Create(taxonomyId, null, "Child2", "Child2", null, 1, "child2", null, null, null, false, null, null, false, null, null).Value;

        entity.Taxons.Add(child1);
        entity.Taxons.Add(child2);

        var result = entity.MapToListItem<TaxonomyListItemResponse>();

        result.TaxonsCount.Should().Be(2);
    }

    [Fact(DisplayName = "MapToDomain (Update): Should preserve presentation when request presentation is null")]
    public void MapToDomain_Update_ShouldPreservePresentation_WhenNull()
    {
        var entity = TaxonomyMethod.Create("Original", "Original Display", 5).Value;

        var request = new TaxonomyRequest { Presentation = null! };

        var result = request.MapToDomain(entity);

        result.IsSuccess.Should().BeTrue();
        entity.Presentation.Should().Be("Original Display");
    }
}
