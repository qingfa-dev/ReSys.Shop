using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.Shared.Mappings;
using Module.Inventory.Features.Admin.StockItems.Shared.Models;
using Module.Inventory.Services;

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

    [Fact(DisplayName = "MapToRestockResponse: Should map StockItem to restock response")]
    public void MapToRestockResponse_ShouldMapCorrectly()
    {
        var stockItem = CreateStockItem(e => e.CountOnHand = 25);
        var movementId = Guid.NewGuid();

        var response = stockItem.MapToRestockResponse<RestockResult>(
            10, 3, 1, 5, movementId);

        response.Should().NotBeNull();
        response.StockItemId.Should().Be(stockItem.Id);
        response.PreviousCountOnHand.Should().Be(10);
        response.NewCountOnHand.Should().Be(25);
        response.BackordersFulfilled.Should().Be(3);
        response.PartiallyFulfilled.Should().Be(1);
        response.RemainingQuantity.Should().Be(5);
        response.MovementId.Should().Be(movementId);
    }

    [Fact(DisplayName = "MapToSummaryItem: Should map StockItem to LocationStockInfo")]
    public void MapToSummaryItem_ShouldMapCorrectly()
    {
        var stockLocation = new StockLocation
        {
            Id = Guid.NewGuid(),
            Name = "Warehouse A",
            LowStockThreshold = 5,
        };
        var stockItem = CreateStockItem(e =>
        {
            e.StockLocation = stockLocation;
            e.StockLocationId = stockLocation.Id;
            e.CountOnHand = 3;
        });

        var response = stockItem.MapToSummaryItem(1, 2);

        response.Should().NotBeNull();
        response.LocationId.Should().Be(stockItem.StockLocationId);
        response.LocationName.Should().Be("Warehouse A");
        response.CountOnHand.Should().Be(3);
        response.Reserved.Should().Be(1);
        response.Available.Should().Be(2);
        response.IsLowStock.Should().BeTrue();
    }

    [Fact(DisplayName = "MapToSummaryItem: Should handle null StockLocation")]
    public void MapToSummaryItem_WhenStockLocationIsNull_ShouldUseDefaults()
    {
        var stockItem = CreateStockItem(e =>
        {
            e.StockLocation = null;
            e.CountOnHand = 10;
        });

        var response = stockItem.MapToSummaryItem(2, 8);

        response.LocationName.Should().Be("Unknown");
        response.IsLowStock.Should().BeFalse();
    }

    [Fact(DisplayName = "MapToSummaryItem: Should clamp negative available to zero")]
    public void MapToSummaryItem_WhenAvailableNegative_ShouldClampToZero()
    {
        var stockItem = CreateStockItem(e => e.CountOnHand = 3);

        var response = stockItem.MapToSummaryItem(5, -2);

        response.Available.Should().Be(0);
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
