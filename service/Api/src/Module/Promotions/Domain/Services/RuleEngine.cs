using System.Globalization;
using Microsoft.EntityFrameworkCore;

using Module.Ordering.Domain.Orders;
using Module.Promotions.Domain.PromotionRules;

namespace Module.Promotions.Domain.Services;

public static class RuleEngine
{
    public static bool Evaluate(
        PromotionRule rule,
        Order order,
        Guid userId,
        IReadOnlyList<string> userRoles)
    {
        return rule.Type switch
        {
            PromotionRuleConstant.Types.ItemTotal => EvaluateItemTotal(rule, order),
            PromotionRuleConstant.Types.Product => EvaluateProduct(rule, order),
            PromotionRuleConstant.Types.UserRole => EvaluateUserRole(rule, userRoles),
            PromotionRuleConstant.Types.User => EvaluateUser(rule, userId, order),
            _ => false,
        };
    }

    private static bool EvaluateItemTotal(PromotionRule rule, Order order)
    {
        if (!rule.Preferences.TryGetValue("amount_min", out var minStr))
            return false;

        if (!decimal.TryParse(minStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var minAmount))
            return false;

        return order.ItemTotal >= minAmount;
    }

    private static bool EvaluateProduct(PromotionRule rule, Order order)
    {
        if (!rule.Preferences.TryGetValue("products", out var productsStr))
            return false;

        var productIds = productsStr
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .ToHashSet();

        if (productIds.Count == 0)
            return false;

        return order.LineItems.Any(li => productIds.Contains(li.VariantId));
    }

    private static bool EvaluateUserRole(PromotionRule rule, IReadOnlyList<string> userRoles)
    {
        if (userRoles.Count == 0)
            return false;

        if (!rule.Preferences.TryGetValue("roles", out var rolesStr))
            return false;

        var requiredRoles = rolesStr
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return requiredRoles.Any(r => userRoles.Contains(r, StringComparer.OrdinalIgnoreCase));
    }

    private static bool EvaluateUser(PromotionRule rule, Guid userId, Order order)
    {
        if (!rule.Preferences.TryGetValue("user_ids", out var userIdsStr))
            return false;

        var userIds = userIdsStr
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .ToHashSet();

        return userIds.Contains(userId);
    }
}
