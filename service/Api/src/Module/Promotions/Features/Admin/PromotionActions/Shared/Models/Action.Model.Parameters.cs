namespace Module.Promotions.Features.Admin.PromotionActions.Shared.Models;

public abstract class PromotionActionParameters
{
    public string Type { get; init; } = string.Empty;
    public Dictionary<string, string> Preferences { get; init; } = [];
    public string? CalculatorType { get; init; }
}
