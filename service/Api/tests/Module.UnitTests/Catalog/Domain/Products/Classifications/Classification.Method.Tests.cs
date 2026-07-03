using Module.Catalog.Domain.Products.Classifications;

namespace Module.UnitTests.Catalog.Domain.Products.Classifications;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "Classification")]
public class ClassificationMethodTests
{
    [Fact(DisplayName = "Create: Should return Classification with correct properties")]
    public void Create_WithValidParameters_ShouldReturnClassification()
    {
        var productId = Guid.NewGuid();
        var taxonId = Guid.NewGuid();
        var position = 1;
        var isAutomatic = true;

        var result = ClassificationMethod.Create(productId, taxonId, position, isAutomatic);

        result.IsSuccess.Should().BeTrue();
        result.Value.ProductId.Should().Be(productId);
        result.Value.TaxonId.Should().Be(taxonId);
        result.Value.Position.Should().Be(position);
        result.Value.IsAutomatic.Should().Be(isAutomatic);
    }

    [Fact(DisplayName = "Create: With null product ID should allow null")]
    public void Create_WithNullProductId_ShouldAllowNull()
    {
        var result = ClassificationMethod.Create(null, Guid.NewGuid());

        result.Value.ProductId.Should().BeNull();
        result.Value.TaxonId.Should().NotBeNull();
    }

    [Fact(DisplayName = "Create: With null taxon ID should allow null")]
    public void Create_WithNullTaxonId_ShouldAllowNull()
    {
        var result = ClassificationMethod.Create(Guid.NewGuid(), null);

        result.Value.ProductId.Should().NotBeNull();
        result.Value.TaxonId.Should().BeNull();
    }
}
