namespace Module.Catalog.Features.Admin.Taxons.Rules.Delete;

public static partial class DeleteTaxonRule
{
    public sealed record Request
    {
        public Guid TaxonId { get; init; }
        public Guid RuleId { get; init; }
    }
}
