namespace Module.Catalog.Features.Admin.Taxons.Rules.Shared.Models;

public record TaxonRuleDetailResponse : TaxonRuleParameter
{
    public Guid Id { get; init; }
    public Guid TaxonId { get; init; }
}

public record TaxonRuleListResponse : TaxonRuleParameter
{
    public Guid Id { get; init; }
}