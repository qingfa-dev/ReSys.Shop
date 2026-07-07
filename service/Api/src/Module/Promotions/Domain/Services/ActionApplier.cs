using System.Globalization;

using Module.Ordering.Domain.Adjustments;
using Module.Ordering.Domain.Orders;
using Module.Promotions.Domain.PromotionActions;

namespace Module.Promotions.Domain.Services;

public static class ActionApplier
{
    public static Result<Adjustment> Apply(PromotionAction action, Order order)
    {
        return action.Type switch
        {
            PromotionActionConstant.Types.CreateAdjustment => CreateAdjustment(action, order),
            PromotionActionConstant.Types.FreeShipping => CreateFreeShipping(action, order),
            _ => PromotionActionResult.Errors.InvalidType,
        };
    }

    private static Result<Adjustment> CreateAdjustment(PromotionAction action, Order order)
    {
        if (action.CalculatorType is null)
            return PromotionActionResult.Errors.TypeRequired;

        var amount = ComputeAmount(action.CalculatorType, action.Preferences, order);
        if (amount is null)
            return PromotionActionResult.Errors.InvalidType;

        var adjustment = new Adjustment
        {
            Id = Guid.NewGuid(),
            Label = action.Preferences.GetValueOrDefault("label", "Promotion"),
            Amount = -Math.Abs(amount.Value),
            DisplayAmount = (-Math.Abs(amount.Value)).ToString("F2", CultureInfo.InvariantCulture),
            Eligible = true,
            Included = true,
            State = "closed",
            AdjustableId = order.Id,
            AdjustableType = "Order",
            SourceId = action.Id,
            SourceType = "PromotionAction",
            OrderId = order.Id,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };

        return adjustment;
    }

    private static decimal? ComputeAmount(string calculatorType, Dictionary<string, string> preferences, Order order)
    {
        return calculatorType switch
        {
            PromotionActionConstant.CalculatorTypes.FlatRate => decimal.TryParse(
                preferences.GetValueOrDefault("amount"), NumberStyles.Any, CultureInfo.InvariantCulture, out var a) ? a : 0m,
            PromotionActionConstant.CalculatorTypes.Percent => decimal.TryParse(
                preferences.GetValueOrDefault("percent"), NumberStyles.Any, CultureInfo.InvariantCulture, out var p)
                ? Math.Round(order.ItemTotal * p / 100m, 2) : 0m,
            _ => null,
        };
    }

    private static Result<Adjustment> CreateFreeShipping(PromotionAction action, Order order)
    {
        var shipmentTotal = order.ShipmentTotal;

        var adjustment = new Adjustment
        {
            Id = Guid.NewGuid(),
            Label = "Free Shipping",
            Amount = -shipmentTotal,
            DisplayAmount = (-shipmentTotal).ToString("F2", CultureInfo.InvariantCulture),
            Eligible = true,
            Included = true,
            State = "closed",
            AdjustableId = order.Id,
            AdjustableType = "Order",
            SourceId = action.Id,
            SourceType = "PromotionAction",
            OrderId = order.Id,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };

        return adjustment;
    }
}
