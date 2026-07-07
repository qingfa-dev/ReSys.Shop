namespace Module.Promotions.Features.Admin.PromotionRules.Shared.Models;

public abstract class PromotionRuleParameters
{
    public string Type { get; init; } = string.Empty;
    public Dictionary<string, string> Preferences { get; init; } = [];
}
