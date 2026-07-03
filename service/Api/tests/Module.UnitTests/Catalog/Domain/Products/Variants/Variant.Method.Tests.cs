using Module.Catalog.Domain.Products.Variants;

namespace Module.UnitTests.Catalog.Domain.Products.Variants;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "Variant")]
public class VariantMethodTests
{
    [Theory(DisplayName = "Create: Should return Variant with correct properties")]
    [InlineData("SKU-001", true, 0)]
    public void Create_WithValidParameters_ShouldReturnVariant(string sku, bool isMaster, int position)
    {
        var productId = Guid.NewGuid();
        var id = Guid.NewGuid();

        var result = VariantMethod.Create(productId, sku, isMaster, position, id: id);
        var variant = result.Value;

        result.IsSuccess.Should().BeTrue();
        variant.Should().NotBeNull();
        variant.Id.Should().Be(id);
        variant.ProductId.Should().Be(productId);
        variant.Sku.Should().Be(sku);
        variant.IsMaster.Should().Be(isMaster);
        variant.Position.Should().Be(position);
    }

    [Fact(DisplayName = "Create: With isMaster false should set correctly")]
    public void Create_WithIsMasterFalse_ShouldSetCorrectly()
    {
        var result = VariantMethod.Create(Guid.NewGuid(), "SKU", isMaster: false, position: 5);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsMaster.Should().BeFalse();
        result.Value.Position.Should().Be(5);
    }

    [Fact(DisplayName = "Create: Without id should generate new id")]
    public void Create_WithoutId_ShouldGenerateNewId()
    {
        var result = VariantMethod.Create(Guid.NewGuid(), "SKU");

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBe(Guid.Empty);
    }

    [Theory(DisplayName = "Update: Should update base properties")]
    [InlineData("NEW-SKU", 10, false)]
    public void Update_WithValidParameters_ShouldUpdateProperties(
        string sku,
        int position,
        bool trackInventory)
    {
        var variant = VariantMethod.Create(Guid.NewGuid(), "OLD-SKU", true, 0).Value;
        var result = variant.Update(sku, position, trackInventory);

        result.IsSuccess.Should().BeTrue();
        variant.Sku.Should().Be(sku);
        variant.Position.Should().Be(position);
        variant.TrackInventory.Should().Be(trackInventory);
    }

    [Fact(DisplayName = "Update: Partial update should preserve other properties")]
    public void Update_WithOnlySku_ShouldPreserveOthers()
    {
        var variant = VariantMethod.Create(Guid.NewGuid(), "OLD-SKU", true, 5).Value;
        var result = variant.Update(sku: "NEW-SKU");

        result.IsSuccess.Should().BeTrue();
        variant.Sku.Should().Be("NEW-SKU");
        variant.Position.Should().Be(5);
        variant.TrackInventory.Should().BeTrue();
    }

    [Fact(DisplayName = "Delete: Should mark as deleted")]
    public void Delete_WhenCalled_ShouldSetIsDeleted()
    {
        var variant = VariantMethod.Create(Guid.NewGuid(), "SKU", true, 0).Value;

        var result = variant.Delete("admin");

        result.IsSuccess.Should().BeTrue();
        variant.IsDeleted.Should().BeTrue();
        variant.DeletedBy.Should().Be("admin");
        variant.DeletedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "Delete: When already deleted should return Ok")]
    public void Delete_WhenAlreadyDeleted_ShouldReturnOk()
    {
        var variant = VariantMethod.Create(Guid.NewGuid(), "SKU", true, 0).Value;
        variant.Delete("admin");

        var result = variant.Delete("admin");

        result.IsSuccess.Should().BeTrue();
    }
}
