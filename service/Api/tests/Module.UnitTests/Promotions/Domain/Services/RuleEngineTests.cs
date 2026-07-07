using FluentAssertions;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Domain.LineItems;
using Module.Promotions.Domain.PromotionRules;
using Module.Promotions.Domain.Services;

namespace Module.UnitTests.Promotions.Domain.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Domain", "RuleEngine")]
public class RuleEngineTests
{
    private static Order CreateOrder(decimal itemTotal = 0, List<Guid>? variantIds = null)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            ItemTotal = itemTotal,
            Number = "R123",
            Currency = "USD",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        if (variantIds is not null)
        {
            foreach (var vid in variantIds)
            {
                order.LineItems.Add(new LineItem
                {
                    Id = Guid.NewGuid(),
                    VariantId = vid,
                    Quantity = 1,
                    Price = itemTotal / Math.Max(variantIds.Count, 1),
                    Total = itemTotal / Math.Max(variantIds.Count, 1),
                    Currency = "USD",
                    OrderId = order.Id
                });
            }
        }
        return order;
    }

    // ===== ItemTotal Tests =====

    [Fact(DisplayName = "Evaluate ItemTotal: Should return true when total exceeds min")]
    public void Evaluate_ItemTotal_ShouldReturnTrue_WhenTotalExceedsMin()
    {
        var rule = PromotionRuleExtensions.Create("ItemTotal",
            new Dictionary<string, string> { ["amount_min"] = "100" }).Value;
        var order = CreateOrder(itemTotal: 150);

        var result = RuleEngine.Evaluate(rule, order, Guid.Empty, []);

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "Evaluate ItemTotal: Should return false when total below min")]
    public void Evaluate_ItemTotal_ShouldReturnFalse_WhenTotalBelowMin()
    {
        var rule = PromotionRuleExtensions.Create("ItemTotal",
            new Dictionary<string, string> { ["amount_min"] = "100" }).Value;
        var order = CreateOrder(itemTotal: 50);

        var result = RuleEngine.Evaluate(rule, order, Guid.Empty, []);

        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Evaluate ItemTotal: Should return false when preference missing")]
    public void Evaluate_ItemTotal_ShouldReturnFalse_WhenPreferenceMissing()
    {
        var rule = PromotionRuleExtensions.Create("ItemTotal").Value;
        var order = CreateOrder(itemTotal: 150);

        var result = RuleEngine.Evaluate(rule, order, Guid.Empty, []);

        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Evaluate ItemTotal: Should return false when preference not decimal")]
    public void Evaluate_ItemTotal_ShouldReturnFalse_WhenPreferenceNotDecimal()
    {
        var rule = PromotionRuleExtensions.Create("ItemTotal",
            new Dictionary<string, string> { ["amount_min"] = "abc" }).Value;
        var order = CreateOrder(itemTotal: 150);

        var result = RuleEngine.Evaluate(rule, order, Guid.Empty, []);

        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Evaluate ItemTotal: Should handle zero threshold")]
    public void Evaluate_ItemTotal_ShouldHandleZeroThreshold()
    {
        var rule = PromotionRuleExtensions.Create("ItemTotal",
            new Dictionary<string, string> { ["amount_min"] = "0" }).Value;
        var order = CreateOrder(itemTotal: 0);

        var result = RuleEngine.Evaluate(rule, order, Guid.Empty, []);

        result.Should().BeTrue();
    }

    // ===== Product Tests =====

    [Fact(DisplayName = "Evaluate Product: Should return true when variant matches")]
    public void Evaluate_Product_ShouldReturnTrue_WhenVariantMatches()
    {
        var variantId = Guid.NewGuid();
        var rule = PromotionRuleExtensions.Create("Product",
            new Dictionary<string, string> { ["products"] = variantId.ToString() }).Value;
        var order = CreateOrder(variantIds: [variantId]);

        var result = RuleEngine.Evaluate(rule, order, Guid.Empty, []);

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "Evaluate Product: Should return false when no match")]
    public void Evaluate_Product_ShouldReturnFalse_WhenNoMatch()
    {
        var rule = PromotionRuleExtensions.Create("Product",
            new Dictionary<string, string> { ["products"] = Guid.NewGuid().ToString() }).Value;
        var order = CreateOrder(variantIds: [Guid.NewGuid()]);

        var result = RuleEngine.Evaluate(rule, order, Guid.Empty, []);

        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Evaluate Product: Should return false when preference missing")]
    public void Evaluate_Product_ShouldReturnFalse_WhenPreferenceMissing()
    {
        var rule = PromotionRuleExtensions.Create("Product").Value;
        var order = CreateOrder(variantIds: [Guid.NewGuid()]);

        var result = RuleEngine.Evaluate(rule, order, Guid.Empty, []);

        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Evaluate Product: Should handle comma-separated ids")]
    public void Evaluate_Product_ShouldHandleCommaSeparatedIds()
    {
        var matchId = Guid.NewGuid();
        var rule = PromotionRuleExtensions.Create("Product",
            new Dictionary<string, string> { ["products"] = $"{Guid.NewGuid()},{matchId}" }).Value;
        var order = CreateOrder(variantIds: [matchId]);

        var result = RuleEngine.Evaluate(rule, order, Guid.Empty, []);

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "Evaluate Product: Should handle malformed guids")]
    public void Evaluate_Product_ShouldHandleMalformedGuids()
    {
        var matchId = Guid.NewGuid();
        var rule = PromotionRuleExtensions.Create("Product",
            new Dictionary<string, string> { ["products"] = $"not-a-guid,{matchId}" }).Value;
        var order = CreateOrder(variantIds: [matchId]);

        var result = RuleEngine.Evaluate(rule, order, Guid.Empty, []);

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "Evaluate Product: Should handle empty line items")]
    public void Evaluate_Product_ShouldHandleEmptyLineItems()
    {
        var rule = PromotionRuleExtensions.Create("Product",
            new Dictionary<string, string> { ["products"] = Guid.NewGuid().ToString() }).Value;
        var order = CreateOrder();

        var result = RuleEngine.Evaluate(rule, order, Guid.Empty, []);

        result.Should().BeFalse();
    }

    // ===== UserRole Tests =====

    [Fact(DisplayName = "Evaluate UserRole: Should return true when role matches")]
    public void Evaluate_UserRole_ShouldReturnTrue_WhenRoleMatches()
    {
        var rule = PromotionRuleExtensions.Create("UserRole",
            new Dictionary<string, string> { ["roles"] = "admin" }).Value;

        var result = RuleEngine.Evaluate(rule, null!, Guid.Empty, ["admin"]);

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "Evaluate UserRole: Should return false when no match")]
    public void Evaluate_UserRole_ShouldReturnFalse_WhenNoMatch()
    {
        var rule = PromotionRuleExtensions.Create("UserRole",
            new Dictionary<string, string> { ["roles"] = "vip" }).Value;

        var result = RuleEngine.Evaluate(rule, null!, Guid.Empty, ["user"]);

        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Evaluate UserRole: Should be case insensitive")]
    public void Evaluate_UserRole_ShouldBeCaseInsensitive()
    {
        var rule = PromotionRuleExtensions.Create("UserRole",
            new Dictionary<string, string> { ["roles"] = "Admin" }).Value;

        var result = RuleEngine.Evaluate(rule, null!, Guid.Empty, ["admin"]);

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "Evaluate UserRole: Should return false when roles empty")]
    public void Evaluate_UserRole_ShouldReturnFalse_WhenRolesEmpty()
    {
        var rule = PromotionRuleExtensions.Create("UserRole",
            new Dictionary<string, string> { ["roles"] = "admin" }).Value;

        var result = RuleEngine.Evaluate(rule, null!, Guid.Empty, []);

        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Evaluate UserRole: Should return false when preference missing")]
    public void Evaluate_UserRole_ShouldReturnFalse_WhenPreferenceMissing()
    {
        var rule = PromotionRuleExtensions.Create("UserRole").Value;

        var result = RuleEngine.Evaluate(rule, null!, Guid.Empty, ["admin"]);

        result.Should().BeFalse();
    }

    // ===== User Tests =====

    [Fact(DisplayName = "Evaluate User: Should return true when user id in list")]
    public void Evaluate_User_ShouldReturnTrue_WhenUserIdInList()
    {
        var userId = Guid.NewGuid();
        var rule = PromotionRuleExtensions.Create("User",
            new Dictionary<string, string> { ["user_ids"] = userId.ToString() }).Value;

        var result = RuleEngine.Evaluate(rule, null!, userId, []);

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "Evaluate User: Should return false when not in list")]
    public void Evaluate_User_ShouldReturnFalse_WhenNotInList()
    {
        var rule = PromotionRuleExtensions.Create("User",
            new Dictionary<string, string> { ["user_ids"] = Guid.NewGuid().ToString() }).Value;

        var result = RuleEngine.Evaluate(rule, null!, Guid.NewGuid(), []);

        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Evaluate User: Should handle comma-separated ids")]
    public void Evaluate_User_ShouldHandleCommaSeparatedIds()
    {
        var userId = Guid.NewGuid();
        var rule = PromotionRuleExtensions.Create("User",
            new Dictionary<string, string> { ["user_ids"] = $"{Guid.NewGuid()},{userId}" }).Value;

        var result = RuleEngine.Evaluate(rule, null!, userId, []);

        result.Should().BeTrue();
    }

    // ===== Dispatcher Edge Cases =====

    [Fact(DisplayName = "Evaluate: Should return false for unknown type")]
    public void Evaluate_ShouldReturnFalse_ForUnknownType()
    {
        var rule = PromotionRuleExtensions.Create("NonExistentType").Value;

        var result = RuleEngine.Evaluate(rule, null!, Guid.Empty, []);

        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Evaluate: Should handle null and empty type")]
    public void Evaluate_ShouldHandleNullAndEmptyType()
    {
        var nullType = new PromotionRule { Id = Guid.NewGuid(), Type = null!, Preferences = [], CreatedAtUtc = DateTimeOffset.UtcNow };
        RuleEngine.Evaluate(nullType, null!, Guid.Empty, []).Should().BeFalse();

        var emptyType = new PromotionRule { Id = Guid.NewGuid(), Type = "", Preferences = [], CreatedAtUtc = DateTimeOffset.UtcNow };
        RuleEngine.Evaluate(emptyType, null!, Guid.Empty, []).Should().BeFalse();
    }
}
