using Module.Ordering.Domain.LineItems;

namespace Module.UnitTests.Ordering.Domain.LineItems;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Entity", "LineItem")]
public class LineItemMethodTests
{
    [Fact(DisplayName = "Create: valid params returns success with correct properties")]
    public void Create_WithValidParams_ShouldReturnLineItem()
    {
        var orderId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        var result = LineItemMethod.Create(orderId, variantId, 2, 19.99m);

        result.IsSuccess.Should().BeTrue();
        var item = result.Value;
        item.OrderId.Should().Be(orderId);
        item.VariantId.Should().Be(variantId);
        item.Quantity.Should().Be(2);
        item.Price.Should().Be(19.99m);
        item.CreatedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        item.CreatedBy.Should().Be("System");
    }

    [Fact(DisplayName = "Create: zero quantity returns QuantityExceedsMax error")]
    public void Create_WithZeroQuantity_ShouldFail()
    {
        var result = LineItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), 0, 10m);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("LineItem.Quantity.OutOfRange");
    }

    [Fact(DisplayName = "Create: negative price returns InvalidPrice error")]
    public void Create_WithNegativePrice_ShouldFail()
    {
        var result = LineItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), 1, -5m);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("LineItem.Price.Invalid");
    }

    [Fact(DisplayName = "UpdateQuantity: updates quantity and recalculates total")]
    public void UpdateQuantity_ShouldRecalculate()
    {
        var lineItem = LineItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), 1, 10m).Value;

        var result = lineItem.UpdateQuantity(3);

        result.IsSuccess.Should().BeTrue();
        lineItem.Quantity.Should().Be(3);
        lineItem.Total.Should().Be(3 * 10m + lineItem.AdjustmentTotal);
    }

    [Fact(DisplayName = "RecalculateTotal: computes Total = (Qty * Price) + AdjustmentTotal")]
    public void RecalculateTotal_ShouldComputeCorrectly()
    {
        var lineItem = LineItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), 2, 15m).Value;
        lineItem.AdjustmentTotal = 5m;

        var result = lineItem.RecalculateTotal();

        result.IsSuccess.Should().BeTrue();
        lineItem.Total.Should().Be((2 * 15m) + 5m);
    }

}
