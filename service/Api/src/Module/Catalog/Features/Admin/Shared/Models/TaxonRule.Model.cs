namespace Module.Catalog.Features.Admin.Shared.Models;

public abstract record TaxonRuleActionParameters
{
    public Guid TaxonId { get; init; }
    public Guid RuleId { get; init; }
}

public abstract record TaxonRuleCollectionParameters
{
    public Guid TaxonId { get; init; }
    public List<SyncItem> Rules { get; init; } = [];
}

public sealed record SyncItem : TaxonRuleRequest
{
    public Guid? Id { get; init; }
}

public abstract record TaxonRuleParameter
{
    public string Type { get; init; } = string.Empty;
    public string MatchPolicy { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public record TaxonRuleRequest : TaxonRuleParameter;

public record TaxonRuleDetailResponse : TaxonRuleParameter
{
    public Guid Id { get; init; }
    public Guid TaxonId { get; init; }
}

public record TaxonRuleListResponse : TaxonRuleParameter
{
    public Guid Id { get; init; }
}
