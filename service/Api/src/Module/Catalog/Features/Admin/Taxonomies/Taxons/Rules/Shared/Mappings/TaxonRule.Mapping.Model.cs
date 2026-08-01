using Module.Catalog.Domain.Taxonomies.Taxons.Rules;
using Module.Catalog.Features.Admin.Taxons.Rules.Shared.Models;

namespace Module.Catalog.Features.Admin.Taxons.Rules.Shared.Mappings;

public static partial class TaxonRuleMapping
{
    public static T MapToDetail<T>(this TaxonRule rule) where T : TaxonRuleDetailResponse, new()
    {
        return new T
        {
            Id = rule.Id,
            TaxonId = rule.TaxonId,
            Type = rule.Type.ToEnumMemberValue(),
            MatchPolicy = rule.MatchPolicy.ToEnumMemberValue(),
            Value = rule.Value,
        };
    }

    public static T MapToListItem<T>(this TaxonRule rule) where T : TaxonRuleListResponse, new()
    {
        return new T
        {
            Id = rule.Id,
            Type = rule.Type.ToEnumMemberValue(),
            MatchPolicy = rule.MatchPolicy.ToEnumMemberValue(),
            Value = rule.Value,
        };
    }
}