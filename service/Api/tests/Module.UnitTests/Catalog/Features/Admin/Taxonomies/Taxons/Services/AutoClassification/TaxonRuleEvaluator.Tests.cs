using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.AutoClassification;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.AutoClassification.Abstractions;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Services.AutoClassification;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "AutoClassification")]
[Trait("Implementation", "Standard")]
public class TaxonRuleEvaluatorTests : TaxonRuleEvaluatorTestsBase
{
    protected override ITaxonRuleEvaluator CreateEvaluator() => new TaxonRuleEvaluator();
}
