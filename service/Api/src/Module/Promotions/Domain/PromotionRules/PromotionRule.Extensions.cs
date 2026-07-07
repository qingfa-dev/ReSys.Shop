namespace Module.Promotions.Domain.PromotionRules;

public static class PromotionRuleExtensions
{
    #region Factory Methods
    /// <summary>Creates a new promotion rule of the specified type with optional preferences.</summary>
    /// <param name="type">The fully-qualified rule type name.</param>
    /// <param name="preferences">Optional dictionary of rule configuration preferences.</param>
    /// <param name="promotionId">Optional identifier of the parent promotion.</param>
    /// <param name="id">Optional explicit identifier.</param>
    /// <returns>A Result containing the created PromotionRule on success.</returns>
    // Contract: pre=type is non-null and non-empty, post=entity.Id is not default, throws=none
    public static Result<PromotionRule> Create(
        string type,
        Dictionary<string, string>? preferences = null,
        Guid? promotionId = null,
        Guid? id = null)
    {
        return new PromotionRule
        {
            Id = id ?? Guid.NewGuid(),
            Type = type,
            Preferences = preferences ?? [],
            PromotionId = promotionId ?? Guid.Empty,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };
    }
    #endregion Factory Methods

    #region Methods
    /// <summary>Updates the rule's type and preferences.</summary>
    /// <param name="rule">The rule to update.</param>
    /// <param name="type">Optional new rule type.</param>
    /// <param name="preferences">Optional new preferences (replaces existing).</param>
    /// <returns>A Result indicating success.</returns>
    public static Result Update(this PromotionRule rule,
        string? type = null,
        Dictionary<string, string>? preferences = null)
    {
        rule.Type = type ?? rule.Type;
        if (preferences is not null)
        {
            rule.Preferences = preferences;
        }

        return Result.Ok();
    }

    /// <summary>Sets a single preference value on the rule.</summary>
    /// <param name="rule">The rule to modify.</param>
    /// <param name="key">The preference key.</param>
    /// <param name="value">The preference value.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result SetPreference(this PromotionRule rule, string key, string value)
    {
        rule.Preferences[key] = value;

        return Result.Ok();
    }

    /// <summary>Removes a preference by key from the rule.</summary>
    /// <param name="rule">The rule to modify.</param>
    /// <param name="key">The preference key to remove.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result RemovePreference(this PromotionRule rule, string key)
    {
        rule.Preferences.Remove(key);

        return Result.Ok();
    }
    #endregion Methods
}