namespace Module.Catalog.Domain.Taxons.Rules;

public static class TaxonRuleConstant
{
    public static class Constraints
    {
        public const int TypeMaxLength = 50;
        public const int ValueMaxLength = 255;
        public const int PolicyMaxLength = 50;
    }

    public static class Default
    {
        public const TaxonRuleMatchPolicy MatchPolicy = TaxonRuleMatchPolicy.IsEqualTo;
        public const TaxonRuleType Type = TaxonRuleType.ProductName;
    }


    public static class Feilds
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(TaxonRule.Type),
            nameof(TaxonRule.Value),
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(TaxonRule.Type),
            nameof(TaxonRule.MatchPolicy)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(TaxonRule.Type),
            nameof(TaxonRule.MatchPolicy)
        ];
    }
}