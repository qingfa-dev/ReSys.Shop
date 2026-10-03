using Module.Catalog.Domain.Taxons.Rules;
using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.Catalog.Features.Admin.Shared.Mappings;

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
