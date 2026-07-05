using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.Shared.Mappings;
using Module.Inventory.Features.Admin.StockItems.Shared.Models;

namespace Module.UnitTests.Inventory.Features.Admin.StockItems.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockItemMapping")]
public class StockItemMappingTests
{
    [Fact(DisplayName = "ToDomain: Should map request to domain entity")]
    public void ToDomain_ShouldMapRequestToEntity()
    {
        var request = new StockItemRequest { StockLocationId = Guid.NewGuid(), VariantId = Guid.NewGuid(), CountOnHand = 10, Backorderable = true };

        var result = request.MapToDomain();
        var entity = result.Value;

        result.IsSuccess.Should().BeTrue();
        entity.Should().NotBeNull();
        entity.StockLocationId.Should().Be(request.StockLocationId);
        entity.VariantId.Should().Be(request.VariantId);
        entity.CountOnHand.Should().Be(request.CountOnHand);
        entity.Backorderable.Should().Be(request.Backorderable);
        entity.Id.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "ToDomain: Should use default value for Backorderable")]
    public void ToDomain_WhenBackorderableIsDefault_ShouldUseDefault()
    {
        var request = new StockItemRequest { StockLocationId = Guid.NewGuid(), VariantId = Guid.NewGuid(), CountOnHand = 10 };

        var result = request.MapToDomain();
        var entity = result.Value;

        entity.Backorderable.Should().Be(StockItemConstant.Defaults.Backorderable);
    }

    [Fact(DisplayName = "ToDomain (Update): Should update existing entity")]
    public void ToDomain_Update_ShouldUpdateEntity()
    {
        var request = new StockItemRequest { StockLocationId = Guid.NewGuid(), VariantId = Guid.NewGuid(), CountOnHand = 20, Backorderable = false };

        var entity = StockItemMethod.Create(
            Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = request.MapToDomain(entity);

        result.IsSuccess.Should().BeTrue();
        entity.CountOnHand.Should().Be(20);
        entity.Backorderable.Should().BeFalse();
    }

    [Fact(DisplayName = "ToDetail: Should map entity to detail response")]
    public void ToDetail_ShouldMapEntityToDetail()
    {
        var entity = CreateStockItem();

        var response = entity.MapToDetail<StockItemDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(entity.Id);
        response.StockLocationId.Should().Be(entity.StockLocationId);
        response.VariantId.Should().Be(entity.VariantId);
        response.CountOnHand.Should().Be(entity.CountOnHand);
        response.Backorderable.Should().Be(entity.Backorderable);
        response.CreatedAtUtc.Should().Be(entity.CreatedAtUtc);
        response.ModifiedAtUtc.Should().Be(entity.ModifiedAtUtc);
    }

    [Fact(DisplayName = "ToDetail: Should handle null ModifiedAtUtc")]
    public void ToDetail_WhenModifiedAtNull_ShouldMapCorrectly()
    {
        var entity = CreateStockItem(e => e.ModifiedAtUtc = null);

        var response = entity.MapToDetail<StockItemDetailResponse>();

        response.ModifiedAtUtc.Should().BeNull();
    }

    [Fact(DisplayName = "ToListItem: Should map entity to list item response")]
    public void ToListItem_ShouldMapEntityToList()
    {
        var entity = CreateStockItem();

        var response = entity.MapToListItem<StockItemListItemResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(entity.Id);
        response.StockLocationId.Should().Be(entity.StockLocationId);
        response.VariantId.Should().Be(entity.VariantId);
        response.CountOnHand.Should().Be(entity.CountOnHand);
        response.Backorderable.Should().Be(entity.Backorderable);
        response.CreatedAtUtc.Should().Be(entity.CreatedAtUtc);
    }

    private static StockItem CreateStockItem(Action<StockItem>? configure = null)
    {
        var stockItem = new StockItem
        {
            Id = Guid.NewGuid(),
            StockLocationId = Guid.NewGuid(),
            VariantId = Guid.NewGuid(),
            CountOnHand = 10,
            Backorderable = true,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1),
            ModifiedAtUtc = DateTimeOffset.UtcNow,
        };
        configure?.Invoke(stockItem);
        return stockItem;
    }
}
