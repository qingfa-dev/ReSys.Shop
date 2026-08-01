using Module.Catalog.Features.Admin.Taxons.Services.AutoClassification;
using Module.Catalog.Features.Admin.Taxons.Services.AutoClassification.Abstractions;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Services.AutoClassification;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "AutoClassification")]
[Trait("Implementation", "Expression")]
public class QueryingTaxonRuleEvaluatorTests : TaxonRuleEvaluatorTestsBase
{
    protected override ITaxonRuleEvaluator CreateEvaluator() => new QueryingTaxonRuleEvaluator();
}
