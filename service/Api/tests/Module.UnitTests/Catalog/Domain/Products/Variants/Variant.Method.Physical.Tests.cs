using Module.Catalog.Domain.Variants;

namespace Module.UnitTests.Catalog.Domain.Products.Variants;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "Variant")]
[Trait("Concern", "Physical")]
public class VariantMethodPhysicalTests
{
    [Fact(DisplayName = "UpdatePhysicalSpecs: Should update weight and size")]
    public void UpdatePhysicalSpecs_WithValidParameters_ShouldUpdateSpecs()
    {
        var variant = VariantMethod.Create(Guid.NewGuid(), "SKU", true, 0).Value;
        var result = variant.UpdatePhysicalSpecs(1.5m, WeightUnit.Kg, 10m, 20m, 5m, DimensionUnit.Cm);

        result.IsSuccess.Should().BeTrue();
        variant.Weight.Should().Be(1.5m);
        variant.WeightUnit.Should().Be(WeightUnit.Kg);
        variant.Height.Should().Be(10m);
        variant.Width.Should().Be(20m);
        variant.Depth.Should().Be(5m);
        variant.DimensionsUnit.Should().Be(DimensionUnit.Cm);
    }

    [Fact(DisplayName = "UpdatePhysicalSpecs: Partial update should preserve other values")]
    public void UpdatePhysicalSpecs_WithOnlyWeight_ShouldPreserveOthers()
    {
        var variant = VariantMethod.Create(Guid.NewGuid(), "SKU", true, 0).Value;
        variant.Weight = 1m;
        variant.WeightUnit = WeightUnit.Kg;
        variant.Height = 10m;

        var result = variant.UpdatePhysicalSpecs(weight: 5m);

        result.IsSuccess.Should().BeTrue();
        variant.Weight.Should().Be(5m);
        variant.WeightUnit.Should().Be(WeightUnit.Kg);
        variant.Height.Should().Be(10m);
    }
}
