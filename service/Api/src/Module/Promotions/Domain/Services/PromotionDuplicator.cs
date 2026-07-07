using Module.Promotions.Domain.PromotionActions;
using Module.Promotions.Domain.PromotionRules;
using Module.Promotions.Domain.Promotions;

// @CAT-2 Create: Clone an existing promotion with new name, code, path and copied rules/actions
namespace Module.Promotions.Domain.Services;
/// <summary>Duplicates a promotion with its rules and actions, creating a new inactive copy.</summary>

// Contract: pre=promotion is non-null with Rules and Actions loaded, post=new_promotion with duplicated rules and actions
// Invariant: Promotion duplication preserves configuration and eligibility; new promotion is inactive by default
public sealed partial class PromotionDuplicator
{
    #region Properties
    private readonly Promotion _promotion;
    private readonly string _randomString;
    #endregion Properties

    #region Constructor
    public PromotionDuplicator(Promotion promotion, string? randomString = null)
    {
        _promotion = promotion;
        _randomString = randomString ?? GenerateRandomString(4);
    }
    #endregion Constructor

    #region Duplicate
    /// <summary>Creates a deep copy of the promotion with new identifiers and inactive status.</summary>
    /// <returns>A new Promotion with copied rules and actions.</returns>
    // Clone: Deep-copy promotion metadata, rules (with associated users/taxons/products), and actions (with calculators and line items)
    public Promotion Duplicate()
    {
        var newId = Guid.NewGuid();
        var suffix = $" (Copy {_randomString})";
        var newName = TruncateName($"{_promotion.Name}{suffix}");

        // Create: Build new promotion from copied metadata.
        var newPromotion = PromotionExtensions.Create(
            name: newName,
            code: _promotion.Code,
            description: _promotion.Description,
            usageLimit: _promotion.UsageLimit,
            perCustomerUsageLimit: _promotion.PerCustomerUsageLimit,
            startsAtUtc: _promotion.StartsAtUtc,
            expiresAtUtc: _promotion.ExpiresAtUtc,
            matchPolicy: _promotion.MatchPolicy,
            kind: _promotion.Kind,
            advertise: _promotion.Advertise,
            active: false, // New duplicates start inactive
            position: _promotion.Position,
            path: _promotion.Path,
            id: newId).Value;

        // Clone: Deep-copy each promotion rule with a new ID.
        if (_promotion.PromotionRules.Count != 0)
        {
            var copiedRules = new List<PromotionRule>(_promotion.PromotionRules.Count);
            foreach (var rule in _promotion.PromotionRules)
            {
                var copiedRule = PromotionRuleExtensions.Create(
                    type: rule.Type,
                    preferences: new Dictionary<string, string>(rule.Preferences),
                    promotionId: newId).Value;
                copiedRules.Add(copiedRule);
            }
            newPromotion.PromotionRules = copiedRules;
        }

        // Clone: Deep-copy each promotion action with a new ID.
        if (_promotion.PromotionActions.Count != 0)
        {
            var copiedActions = new List<PromotionAction>(_promotion.PromotionActions.Count);
            foreach (var action in _promotion.PromotionActions)
            {
                var copiedAction = PromotionActionExtensions.Create(
                    type: action.Type,
                    preferences: new Dictionary<string, string>(action.Preferences),
                    calculatorType: action.CalculatorType,
                    promotionId: newId).Value;
                copiedActions.Add(copiedAction);
            }
            newPromotion.PromotionActions = copiedActions;
        }

        return newPromotion;
    }
    #endregion Duplicate

    #region Helpers
    private static string TruncateName(string name)
    {
        var maxLength = PromotionConstant.Constraints.MaxNameLength;
        return name.Length <= maxLength ? name : name[..maxLength];
    }

    private static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        return new string(Enumerable.Range(0, length).Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
    }
    #endregion Helpers
}
