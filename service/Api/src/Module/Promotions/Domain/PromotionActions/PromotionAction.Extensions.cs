namespace Module.Promotions.Domain.PromotionActions;

public static class PromotionActionExtensions
{
    #region Factory Methods
    /// <summary>Creates a new promotion action with the specified type, preferences, and calculator.</summary>
    /// <param name="type">The fully-qualified action type name.</param>
    /// <param name="preferences">Optional dictionary of action configuration preferences.</param>
    /// <param name="calculatorType">Optional calculator type for amount-based actions.</param>
    /// <param name="promotionId">Optional identifier of the parent promotion.</param>
    /// <param name="id">Optional explicit identifier.</param>
    /// <returns>A Result containing the created PromotionAction on success.</returns>
    // Contract: pre=type is non-null and non-empty, post=entity.Id is not default, throws=none
    public static Result<PromotionAction> Create(
        string type,
        Dictionary<string, string>? preferences = null,
        string? calculatorType = null,
        Guid? promotionId = null,
        Guid? id = null)
    {
        return new PromotionAction
        {
            Id = id ?? Guid.NewGuid(),
            Type = type,
            Preferences = preferences ?? [],
            CalculatorType = calculatorType,
            PromotionId = promotionId ?? Guid.Empty,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };
    }
    #endregion Factory Methods

    #region Methods
    /// <summary>Updates the action's type, preferences, and calculator type.</summary>
    /// <param name="action">The action to update.</param>
    /// <param name="type">Optional new action type.</param>
    /// <param name="preferences">Optional new preferences (replaces existing).</param>
    /// <param name="calculatorType">Optional new calculator type.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result Update(this PromotionAction action,
        string? type = null,
        Dictionary<string, string>? preferences = null,
        string? calculatorType = null)
    {
        action.Type = type ?? action.Type;
        action.CalculatorType = calculatorType ?? action.CalculatorType;
        if (preferences is not null)
        {
            action.Preferences = preferences;
        }

        return Result.Ok();
    }

    /// <summary>Sets a single preference value on the action.</summary>
    /// <param name="action">The action to modify.</param>
    /// <param name="key">The preference key.</param>
    /// <param name="value">The preference value.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result SetPreference(this PromotionAction action, string key, string value)
    {
        action.Preferences[key] = value;

        return Result.Ok();
    }

    /// <summary>Removes a preference by key from the action.</summary>
    /// <param name="action">The action to modify.</param>
    /// <param name="key">The preference key to remove.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result RemovePreference(this PromotionAction action, string key)
    {
        action.Preferences.Remove(key);

        return Result.Ok();
    }
    #endregion Methods
}