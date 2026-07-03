namespace Module.Catalog.Domain.Taxonomies.Taxons.Rules;

public static class TaxonRuleExtensions
{
    #region Factory Methods
    /// <summary>
    /// Creates a new taxon rule for automatic product classification.
    /// </summary>
    /// <param name="taxonId">The parent taxon identifier.</param>
    /// <param name="type">The rule type (e.g. product name, price).</param>
    /// <param name="matchPolicy">The match policy (e.g. equals, contains).</param>
    /// <param name="value">The value to match against.</param>
    /// <param name="id">Optional explicit identifier. Auto-generated if not provided.</param>
    /// <returns>The created TaxonRule.</returns>
    // Contract: pre=taxonId!=Guid.Empty&&value!=null,
    //           post=entity.TaxonId==taxonId&&entity.Value==value, throws=ArgumentException
    public static TaxonRule Create(
        Guid taxonId,
        TaxonRuleType type,
        TaxonRuleMatchPolicy matchPolicy,
        string value,
        Guid? id = null)
    {
        return new TaxonRule
        {
            Id = id ?? Guid.NewGuid(),
            TaxonId = taxonId,
            Type = type,
            MatchPolicy = matchPolicy,
            Value = value,
        };
    }
    #endregion

    #region Methods
    /// <summary>
    /// Updates the taxon rule with the specified properties. Only non-null values are applied.
    /// </summary>
    /// <param name="rule">The taxon rule to update.</param>
    /// <param name="type">Optional new rule type.</param>
    /// <param name="matchPolicy">Optional new match policy.</param>
    /// <param name="value">Optional new value.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result Update(this TaxonRule rule,
        TaxonRuleType? type = null,
        TaxonRuleMatchPolicy? matchPolicy = null,
        string? value = null)
    {
        rule.Type = type ?? rule.Type;
        rule.MatchPolicy = matchPolicy ?? rule.MatchPolicy;
        rule.Value = value ?? rule.Value;

        return Result.Ok();
    }
    #endregion
}