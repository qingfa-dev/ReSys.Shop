using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Domain.Taxonomies.Taxons.Rules;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.AutoClassification.Abstractions;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.AutoClassification;

/// <summary>
/// Pure, stateless evaluation of a product against a taxon's rule set.
/// </summary>
public sealed class TaxonRuleEvaluator : ITaxonRuleEvaluator
{
    /// <inheritdoc />
    public bool Evaluate(Product product, Taxon taxon)
    {
        // Validate: Basic requirements for evaluation
        if (!taxon.Automatic || taxon.TaxonRules.Count == 0)
        {
            return false;
        }

        // Evaluate: Based on match policy (AND/OR)
        return taxon.RulesMatchPolicy switch
        {
            TaxonMatchPolicy.All => taxon.TaxonRules.All(rule => Matches(product, rule)),
            TaxonMatchPolicy.Any => taxon.TaxonRules.Any(rule => Matches(product, rule)),
            _ => false
        };
    }

    private static bool Matches(Product product, TaxonRule rule)
    {
        // Route: Evaluation based on rule type
        return rule.Type switch
        {
            TaxonRuleType.ProductName => CompareString(product.Name, rule.MatchPolicy, rule.Value),
            TaxonRuleType.ProductSku => CompareString(GetMasterVariant(product)?.Sku, rule.MatchPolicy, rule.Value),
            TaxonRuleType.ProductDescription => CompareString(product.Description, rule.MatchPolicy, rule.Value),
            TaxonRuleType.ProductPrice => CompareDecimal(GetMasterVariant(product)?.Price, rule.MatchPolicy, rule.Value),
            TaxonRuleType.ProductWeight => CompareDecimal(GetMasterVariant(product)?.Weight, rule.MatchPolicy, rule.Value),
            TaxonRuleType.ProductAvailable => CompareBool(product.Status == ProductStatus.Active, rule.MatchPolicy, rule.Value),
            TaxonRuleType.ProductArchived => CompareBool(product.Status == ProductStatus.Archived, rule.MatchPolicy, rule.Value),
            TaxonRuleType.VariantPrice => product.Variants.Any(v => CompareDecimal(v.Price, rule.MatchPolicy, rule.Value)),
            TaxonRuleType.VariantSku => product.Variants.Any(v => CompareString(v.Sku, rule.MatchPolicy, rule.Value)),
            TaxonRuleType.ProductStatus => CompareEnum(product.Status, rule.MatchPolicy, rule.Value),
            _ => false
        };
    }

    private static Domain.Products.Variants.Variant? GetMasterVariant(Product product)
        => product.Variants.FirstOrDefault(v => v.IsMaster);

    private static bool CompareString(string? actual, TaxonRuleMatchPolicy policy, string expected)
    {
        actual ??= string.Empty;
        return policy switch
        {
            TaxonRuleMatchPolicy.IsEqualTo => string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
            TaxonRuleMatchPolicy.IsNotEqualTo => !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
            TaxonRuleMatchPolicy.Contains => actual.Contains(expected, StringComparison.OrdinalIgnoreCase),
            TaxonRuleMatchPolicy.DoesNotContain => !actual.Contains(expected, StringComparison.OrdinalIgnoreCase),
            TaxonRuleMatchPolicy.StartsWith => actual.StartsWith(expected, StringComparison.OrdinalIgnoreCase),
            TaxonRuleMatchPolicy.EndsWith => actual.EndsWith(expected, StringComparison.OrdinalIgnoreCase),
            TaxonRuleMatchPolicy.In => expected.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                        .Any(s => string.Equals(actual, s, StringComparison.OrdinalIgnoreCase)),
            TaxonRuleMatchPolicy.NotIn => !expected.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                        .Any(s => string.Equals(actual, s, StringComparison.OrdinalIgnoreCase)),
            TaxonRuleMatchPolicy.IsNull => string.IsNullOrEmpty(actual),
            TaxonRuleMatchPolicy.IsNotNull => !string.IsNullOrEmpty(actual),
            _ => false
        };
    }

    private static bool CompareDecimal(decimal? actual, TaxonRuleMatchPolicy policy, string expected)
    {
        // Handle: Null checks
        if (policy == TaxonRuleMatchPolicy.IsNull) return actual == null;
        if (policy == TaxonRuleMatchPolicy.IsNotNull) return actual != null;

        // Parse: Expected value
        if (!decimal.TryParse(expected, out var expectedValue)) return false;
        if (actual == null) return false;

        // Compare: Numeric values
        return policy switch
        {
            TaxonRuleMatchPolicy.IsEqualTo => actual == expectedValue,
            TaxonRuleMatchPolicy.IsNotEqualTo => actual != expectedValue,
            TaxonRuleMatchPolicy.GreaterThan => actual > expectedValue,
            TaxonRuleMatchPolicy.LessThan => actual < expectedValue,
            TaxonRuleMatchPolicy.GreaterThanOrEqual => actual >= expectedValue,
            TaxonRuleMatchPolicy.LessThanOrEqual => actual <= expectedValue,
            _ => false
        };
    }

    private static bool CompareBool(bool actual, TaxonRuleMatchPolicy policy, string expected)
    {
        if (!bool.TryParse(expected, out var expectedValue)) return false;
        return policy switch
        {
            TaxonRuleMatchPolicy.IsEqualTo => actual == expectedValue,
            TaxonRuleMatchPolicy.IsNotEqualTo => actual != expectedValue,
            _ => false
        };
    }

    private static bool CompareEnum<T>(T actual, TaxonRuleMatchPolicy policy, string expected) where T : struct, Enum
    {
        if (!Enum.TryParse<T>(expected, true, out var expectedValue)) return false;
        return policy switch
        {
            TaxonRuleMatchPolicy.IsEqualTo => EqualityComparer<T>.Default.Equals(actual, expectedValue),
            TaxonRuleMatchPolicy.IsNotEqualTo => !EqualityComparer<T>.Default.Equals(actual, expectedValue),
            _ => false
        };
    }
}
