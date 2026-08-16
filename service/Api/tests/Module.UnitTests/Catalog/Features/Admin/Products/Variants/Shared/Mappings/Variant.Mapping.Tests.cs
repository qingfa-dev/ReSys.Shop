using Module.Catalog.Domain.Variants;
using Module.Catalog.Features.Admin.Shared.Mappings;
using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Variant")]
[Trait("Concern", "Mapping")]
public class VariantMappingTests
{
    [Fact(DisplayName = "MapToDomain: Should map VariantRequest to new Variant entity")]
    public void MapToDomain_Create_ShouldMapRequestToEntity()
    {
        var productId = Guid.NewGuid();
        var request = new VariantRequest
        {
            Sku = "SKU-001",
            IsMaster = true,
            Position = 1,
        };

        var result = request.MapToDomain(productId);
        var entity = result.Value;

        result.IsSuccess.Should().BeTrue();
        entity.Should().NotBeNull();
        entity.ProductId.Should().Be(productId);
        entity.Sku.Should().Be(request.Sku);
        entity.IsMaster.Should().Be(request.IsMaster);
        entity.Position.Should().Be(request.Position);
    }

    [Fact(DisplayName = "MapToDomain (Update): Should update all variant fields")]
    public void MapToDomain_Update_ShouldUpdateAllFields()
    {
        var entity = VariantMethod.Create(Guid.NewGuid(), "OLD-SKU", isMaster: true, position: 0).Value;
        var request = new VariantRequest
        {
            Sku = "NEW-SKU",
            Position = 5,
            TrackInventory = false,
            Price = 29.99m,
            CostPrice = 15.00m,
            CostCurrency = "USD",
            Weight = 1.5m,
            WeightUnit = "kg",
            Height = 10m,
            Width = 20m,
            Depth = 5m,
            DimensionsUnit = "cm",
        };

        var result = request.MapToDomain(entity);

        result.IsSuccess.Should().BeTrue();
        entity.Sku.Should().Be("NEW-SKU");
        entity.Position.Should().Be(5);
        entity.TrackInventory.Should().BeFalse();
        entity.Price.Should().Be(29.99m);
        entity.CostPrice.Should().Be(15.00m);
        entity.CostCurrency.Should().Be("USD");
        entity.Weight.Should().Be(1.5m);
        entity.WeightUnit.Should().Be(WeightUnit.Kg);
        entity.Height.Should().Be(10m);
        entity.Width.Should().Be(20m);
        entity.Depth.Should().Be(5m);
        entity.DimensionsUnit.Should().Be(DimensionUnit.Cm);
    }

    [Fact(DisplayName = "MapToDomain (Update): Partial update should preserve unchanged fields")]
    public void MapToDomain_Update_ShouldPreserveOtherFields()
    {
        var entity = VariantMethod.Create(Guid.NewGuid(), "OLD-SKU", isMaster: true, position: 0).Value;
        entity.Price = 10m;
        entity.CostPrice = 5m;
        entity.CostCurrency = "USD";

        var request = new VariantRequest
        {
            Sku = "NEW-SKU",
            Position = 3,
            TrackInventory = entity.TrackInventory,
            Price = entity.Price,
            CostPrice = entity.CostPrice,
            CostCurrency = entity.CostCurrency,
        };

        var result = request.MapToDomain(entity);

        result.IsSuccess.Should().BeTrue();
        entity.Sku.Should().Be("NEW-SKU");
        entity.Position.Should().Be(3);
        entity.Price.Should().Be(10m);
        entity.CostPrice.Should().Be(5m);
        entity.CostCurrency.Should().Be("USD");
    }

    [Fact(DisplayName = "MapToDetail: Should map Variant entity to VariantDetailResponse")]
    public void MapToDetail_ShouldMapEntityToResponse()
    {
        var variantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var entity = VariantMethod.Create(productId, "SKU-001", isMaster: true, position: 0, id: variantId).Value;
        entity.Price = 19.99m;
        entity.CostPrice = 10m;
        entity.CostCurrency = "USD";

        var result = entity.MapToDetail<VariantDetailResponse>();

        result.Should().NotBeNull();
        result.Id.Should().Be(variantId);
        result.ProductId.Should().Be(productId);
        result.IsMaster.Should().BeTrue();
        result.Sku.Should().Be("SKU-001");
        result.Position.Should().Be(0);
        result.Price.Should().Be(19.99m);
        result.CostPrice.Should().Be(10m);
        result.CostCurrency.Should().Be("USD");
    }

    [Fact(DisplayName = "MapToListItem: Should map Variant entity to VariantListItemResponse")]
    public void MapToListItem_ShouldMapEntityToResponse()
    {
        var variantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var entity = VariantMethod.Create(productId, "SKU-001", isMaster: false, position: 2, id: variantId).Value;

        var result = entity.MapToListItem<VariantListItemResponse>();

        result.Should().NotBeNull();
        result.Id.Should().Be(variantId);
        result.ProductId.Should().Be(productId);
        result.IsMaster.Should().BeFalse();
        result.Sku.Should().Be("SKU-001");
        result.Position.Should().Be(2);
    }
}
