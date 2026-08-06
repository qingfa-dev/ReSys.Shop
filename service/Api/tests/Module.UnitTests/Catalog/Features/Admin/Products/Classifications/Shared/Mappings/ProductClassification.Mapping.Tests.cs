using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Products.Classifications.Shared.Models;
using Module.Catalog.Features.Admin.Products.ProductClassifications.Shared.Mappings;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Classifications.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductClassificationMapping")]
public class ProductClassificationMappingTests
{
    [Fact(DisplayName = "ToDomain: Should map assignment item to classification")]
    public void ToDomain_ShouldMapItemToClassification()
    {
        var productId = Guid.NewGuid();
        var item = new ProductClassificationAssignmentItem
        {
            TaxonId = Guid.NewGuid(),
            Position = 2,
        };

        var result = item.MapToDomain(productId);
        var classification = result.Value;

        result.IsSuccess.Should().BeTrue();
        classification.Should().NotBeNull();
        classification.ProductId.Should().Be(productId);
        classification.TaxonId.Should().Be(item.TaxonId);
        classification.Position.Should().Be(item.Position);
    }

    [Fact(DisplayName = "ToDomain (Update): Should update position on existing entity")]
    public void ToDomain_Update_ShouldUpdatePosition()
    {
        var classification = ClassificationMethod.Create(
            Guid.NewGuid(), Guid.NewGuid(), position: 1).Value;

        var item = new ProductClassificationAssignmentItem
        {
            TaxonId = Guid.NewGuid(),
            Position = 10,
        };

        item.MapToDomain(classification);

        classification.Position.Should().Be(10);
    }

    [Fact(DisplayName = "ToListItem: Should map taxon to list item response when assigned")]
    public void ToListItem_WhenAssigned_ShouldMapCorrectly()
    {
        var taxon = new Taxon
        {
            Id = Guid.NewGuid(),
            Name = "Clothing",
            PrettyName = "Clothing > T-Shirts",
        };

        var response = taxon.MapToClassificationListItem<ClassificationItemResponse>(
            isAssigned: true, position: 3);

        response.Should().NotBeNull();
        response.TaxonId.Should().Be(taxon.Id);
        response.Name.Should().Be(taxon.Name);
        response.PrettyName.Should().Be(taxon.PrettyName);
        response.IsAssigned.Should().BeTrue();
        response.Position.Should().Be(3);
    }

    [Fact(DisplayName = "ToListItem: Should map taxon when not assigned")]
    public void ToListItem_WhenNotAssigned_ShouldMapWithZeroPosition()
    {
        var taxon = new Taxon
        {
            Id = Guid.NewGuid(),
            Name = "Accessories",
            PrettyName = "Accessories",
        };

        var response = taxon.MapToClassificationListItem<ClassificationItemResponse>(
            isAssigned: false, position: 5);

        response.IsAssigned.Should().BeFalse();
        response.Position.Should().Be(0);
        response.TaxonId.Should().Be(taxon.Id);
    }

    [Fact(DisplayName = "ToListItem: Should handle null PrettyName")]
    public void ToListItem_WhenPrettyNameIsNull_ShouldMapCorrectly()
    {
        var taxon = new Taxon
        {
            Id = Guid.NewGuid(),
            Name = "Shoes",
            PrettyName = null!,
        };

        var response = taxon.MapToClassificationListItem<ClassificationItemResponse>(
            isAssigned: true, position: 1);

        response.PrettyName.Should().BeNull();
    }
}
