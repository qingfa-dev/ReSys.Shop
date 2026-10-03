using Shared.Application.Domain.Models;

namespace Module.Catalog.Domain.Taxons.Rules;

/// <summary>
/// Represents a matching rule that determines which products are automatically assigned to a taxon.
/// </summary>
// Invariant: TaxonId != Guid.Empty; Value != null; Type is a valid TaxonRuleType
public sealed partial class TaxonRule : Entity
{
    #region Properties
    public Guid TaxonId { get; set; }
    public TaxonRuleType Type { get; set; }
    public string Value { get; set; } = string.Empty;
    public TaxonRuleMatchPolicy MatchPolicy { get; set; }
    #endregion Properties

    #region Relationships
    public Taxon Taxon { get; set; } = null!;
    #endregion Relationships

    #region Constructor
    internal TaxonRule() { }
    #endregion Constructor
}