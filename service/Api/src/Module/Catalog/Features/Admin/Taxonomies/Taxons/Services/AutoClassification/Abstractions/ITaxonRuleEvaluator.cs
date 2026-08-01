using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Taxonomies.Taxons;

namespace Module.Catalog.Features.Admin.Taxons.Services.AutoClassification.Abstractions;


/// <summary>
/// Pure, stateless evaluation of a product against a taxon's rule set.
/// No database access — registered as Singleton and safe to use in tight loops.
/// </summary>
public interface ITaxonRuleEvaluator
{
    /// <summary>
    /// Returns <c>true</c> when the product satisfies the taxon's rules,
    /// honoring <see cref="Taxon.RulesMatchPolicy"/> (All = AND, Any = OR).
    ///
    /// Prerequisites: <paramref name="taxon"/> must have <c>TaxonRules</c> loaded.
    /// Returns <c>false</c> immediately when <c>taxon.Automatic == false</c>
    /// or when <c>TaxonRules</c> is empty.
    /// </summary>
    bool Evaluate(Product product, Taxon taxon);
}