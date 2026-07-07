using FluentAssertions;
using Module.Ordering.Domain.Orders;
using Module.Promotions.Domain.PromotionActions;
using Module.Promotions.Domain.Services;

namespace Module.UnitTests.Promotions.Domain.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Domain", "ActionApplier")]
public class ActionApplierTests
{
    private static Order CreateOrder(decimal itemTotal = 100, decimal shipmentTotal = 0)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            ItemTotal = itemTotal,
            ShipmentTotal = shipmentTotal,
            Total = itemTotal,
            Number = "R123",
            Currency = "USD",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    [Fact(DisplayName = "Apply CreateAdjustment FlatRate: Should create adjustment")]
    public void Apply_CreateAdjustment_FlatRate_ShouldCreateAdjustment()
    {
        var action = PromotionActionExtensions.Create("CreateAdjustment",
            new Dictionary<string, string> { ["amount"] = "10", ["label"] = "Discount" },
            calculatorType: "FlatRate").Value;
        var order = CreateOrder();

        var result = ActionApplier.Apply(action, order);

        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(-10m);
        result.Value.Label.Should().Be("Discount");
        result.Value.SourceType.Should().Be("PromotionAction");
        result.Value.SourceId.Should().Be(action.Id);
        result.Value.OrderId.Should().Be(order.Id);
        result.Value.Eligible.Should().BeTrue();
        result.Value.State.Should().Be("closed");
    }

    [Fact(DisplayName = "Apply CreateAdjustment FlatRate: Should default label")]
    public void Apply_CreateAdjustment_FlatRate_ShouldDefaultLabel()
    {
        var action = PromotionActionExtensions.Create("CreateAdjustment",
            new Dictionary<string, string> { ["amount"] = "10" },
            calculatorType: "FlatRate").Value;
        var order = CreateOrder();

        var result = ActionApplier.Apply(action, order);

        result.IsSuccess.Should().BeTrue();
        result.Value.Label.Should().Be("Promotion");
    }

    [Fact(DisplayName = "Apply CreateAdjustment Percent: Should compute percentage")]
    public void Apply_CreateAdjustment_Percent_ShouldCreateAdjustment()
    {
        var action = PromotionActionExtensions.Create("CreateAdjustment",
            new Dictionary<string, string> { ["percent"] = "10" },
            calculatorType: "Percent").Value;
        var order = CreateOrder(itemTotal: 100);

        var result = ActionApplier.Apply(action, order);

        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(-10m);
    }

    [Fact(DisplayName = "Apply CreateAdjustment: Should return failure when calculator type null")]
    public void Apply_CreateAdjustment_ShouldReturnFailure_WhenCalculatorTypeNull()
    {
        var action = PromotionActionExtensions.Create("CreateAdjustment",
            calculatorType: null).Value;
        var order = CreateOrder();

        var result = ActionApplier.Apply(action, order);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact(DisplayName = "Apply CreateAdjustment: Should return failure when calculator type unknown")]
    public void Apply_CreateAdjustment_ShouldReturnFailure_WhenCalculatorTypeUnknown()
    {
        var action = PromotionActionExtensions.Create("CreateAdjustment",
            calculatorType: "InvalidCalc").Value;
        var order = CreateOrder();

        var result = ActionApplier.Apply(action, order);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact(DisplayName = "Apply FreeShipping: Should create adjustment with negative shipment total")]
    public void Apply_FreeShipping_ShouldCreateAdjustmentWithNegativeShipmentTotal()
    {
        var action = PromotionActionExtensions.Create("FreeShipping").Value;
        var order = CreateOrder(shipmentTotal: 15.99m);

        var result = ActionApplier.Apply(action, order);

        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(-15.99m);
        result.Value.Label.Should().Be("Free Shipping");
    }

    [Fact(DisplayName = "Apply FreeShipping: Should handle zero shipment total")]
    public void Apply_FreeShipping_ShouldHandleZeroShipmentTotal()
    {
        var action = PromotionActionExtensions.Create("FreeShipping").Value;
        var order = CreateOrder(shipmentTotal: 0);

        var result = ActionApplier.Apply(action, order);

        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(0);
    }

    [Fact(DisplayName = "Apply: Should return failure when action type unknown")]
    public void Apply_ShouldReturnFailure_WhenActionTypeUnknown()
    {
        var action = PromotionActionExtensions.Create("Invalid").Value;
        var order = CreateOrder();

        var result = ActionApplier.Apply(action, order);

        result.IsSuccess.Should().BeFalse();
    }
}
