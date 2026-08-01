using Module.Catalog.Domain.Taxonomies.Taxons.Rules;
using Module.Catalog.Features.Admin.Taxons.Rules.Shared.Models;

namespace Module.Catalog.Features.Admin.Taxons.Rules.Shared.Mappings;

public static partial class TaxonRuleMapping
{
    public static TaxonRule ToEntity<T>(this T request, Guid taxonId) where T : TaxonRuleRequest
    {
        return TaxonRuleExtensions.Create(
            taxonId,
            EnumExtensions.FromEnumMemberValue<TaxonRuleType>(request.Type),
            EnumExtensions.FromEnumMemberValue<TaxonRuleMatchPolicy>(request.MatchPolicy),
            request.Value);
    }

    public static void ToEntity<T>(this T request, TaxonRule rule) where T : TaxonRuleRequest
    {
        rule.Update(
            EnumExtensions.FromEnumMemberValue<TaxonRuleType>(request.Type),
            EnumExtensions.FromEnumMemberValue<TaxonRuleMatchPolicy>(request.MatchPolicy),
            request.Value);
    }
}