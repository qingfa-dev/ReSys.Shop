using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Inventory.Features.Admin.StockMovements.Shared.Mappings;
using Module.Inventory.Features.Admin.StockMovements.Shared.Models;

namespace Module.UnitTests.Inventory.Features.Admin.StockMovements.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockMovementMapping")]
public class StockMovementMappingTests
{
    [Fact(DisplayName = "ToDetail: Should map entity to detail response")]
    public void ToDetail_ShouldMapEntityToDetail()
    {
        var entity = CreateStockMovement();

        var response = entity.MapToDetail<StockMovementDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(entity.Id);
        response.StockItemId.Should().Be(entity.StockItemId);
        response.Quantity.Should().Be(entity.Quantity);
        response.PreviousCountOnHand.Should().Be(entity.PreviousCountOnHand);
        response.Action.Should().Be(entity.Action);
        response.Reason.Should().Be(entity.Reason);
        response.OriginatorType.Should().Be(entity.OriginatorType);
        response.OriginatorId.Should().Be(entity.OriginatorId);
        response.CreatedAtUtc.Should().Be(entity.CreatedAtUtc);
    }

    [Fact(DisplayName = "ToDetail: Should handle null optional fields")]
    public void ToDetail_WhenOptionalFieldsAreNull_ShouldMapCorrectly()
    {
        var entity = CreateStockMovement(e =>
        {
            e.Action = null;
            e.Reason = null;
            e.OriginatorType = null;
            e.OriginatorId = null;
        });

        var response = entity.MapToDetail<StockMovementDetailResponse>();

        response.Action.Should().BeNull();
        response.Reason.Should().BeNull();
        response.OriginatorType.Should().BeNull();
        response.OriginatorId.Should().BeNull();
    }

    [Fact(DisplayName = "ToDetail: Should handle negative quantity")]
    public void ToDetail_WhenQuantityIsNegative_ShouldMapCorrectly()
    {
        var entity = CreateStockMovement(e =>
        {
            e.Quantity = -5;
            e.PreviousCountOnHand = 10;
        });

        var response = entity.MapToDetail<StockMovementDetailResponse>();

        response.Quantity.Should().Be(-5);
        response.PreviousCountOnHand.Should().Be(10);
    }

    [Fact(DisplayName = "ToListItem: Should map entity to list item response")]
    public void ToListItem_ShouldMapEntityToList()
    {
        var entity = CreateStockMovement();

        var response = entity.MapToListItem<StockMovementListItemResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(entity.Id);
        response.StockItemId.Should().Be(entity.StockItemId);
        response.Quantity.Should().Be(entity.Quantity);
        response.PreviousCountOnHand.Should().Be(entity.PreviousCountOnHand);
        response.Action.Should().Be(entity.Action);
        response.Reason.Should().Be(entity.Reason);
        response.OriginatorType.Should().Be(entity.OriginatorType);
        response.OriginatorId.Should().Be(entity.OriginatorId);
        response.CreatedAtUtc.Should().Be(entity.CreatedAtUtc);
    }

    [Fact(DisplayName = "ToListItem: Should handle edge case with zero quantity")]
    public void ToListItem_WhenQuantityIsZero_ShouldMapCorrectly()
    {
        var entity = CreateStockMovement(e =>
        {
            e.Quantity = 0;
            e.PreviousCountOnHand = 0;
        });

        var response = entity.MapToListItem<StockMovementListItemResponse>();

        response.Quantity.Should().Be(0);
        response.PreviousCountOnHand.Should().Be(0);
    }

    private static StockMovement CreateStockMovement(Action<StockMovement>? configure = null)
    {
        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            StockItemId = Guid.NewGuid(),
            Quantity = 5,
            PreviousCountOnHand = 10,
            Action = "sold",
            Reason = "Order fulfillment",
            OriginatorType = "Order",
            OriginatorId = Guid.NewGuid(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
        configure?.Invoke(movement);
        return movement;
    }
}
