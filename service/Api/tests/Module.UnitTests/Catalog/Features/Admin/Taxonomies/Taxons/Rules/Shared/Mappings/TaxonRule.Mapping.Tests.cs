using Module.Catalog.Domain.Taxonomies.Taxons.Rules;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Shared.Mappings;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Shared.Models;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonRuleMapping")]
public class TaxonRuleMappingTests
{
    [Fact(DisplayName = "ToEntity: Should map request to domain entity")]
    public void ToEntity_ShouldMapRequestToEntity()
    {
        var taxonId = Guid.NewGuid();
        var request = new TaxonRuleRequest
        {
            Type = "product_name",
            MatchPolicy = "is_equal_to",
            Value = "T-Shirt",
        };

        var rule = request.ToEntity(taxonId);

        rule.Should().NotBeNull();
        rule.TaxonId.Should().Be(taxonId);
        rule.Value.Should().Be(request.Value);
        rule.Id.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "ToEntity (Update): Should update existing entity from request")]
    public void ToEntity_Update_ShouldUpdateEntity()
    {
        var rule = TaxonRuleExtensions.Create(
            Guid.NewGuid(),
            TaxonRuleType.ProductName,
            TaxonRuleMatchPolicy.IsEqualTo,
            "Old Value");

        var request = new TaxonRuleRequest
        {
            Type = "product_price",
            MatchPolicy = "greater_than",
            Value = "50",
        };

        request.ToEntity(rule);

        rule.Value.Should().Be("50");
    }

    [Fact(DisplayName = "ToDetail: Should map entity to detail response")]
    public void ToDetail_ShouldMapEntityToDetail()
    {
        var rule = CreateTaxonRule();

        var response = rule.MapToDetail<TaxonRuleDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(rule.Id);
        response.TaxonId.Should().Be(rule.TaxonId);
        response.Value.Should().Be(rule.Value);
    }

    [Fact(DisplayName = "ToListItem: Should map entity to list item response")]
    public void ToListItem_ShouldMapEntityToList()
    {
        var rule = CreateTaxonRule();

        var response = rule.MapToListItem<TaxonRuleListResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(rule.Id);
        response.Value.Should().Be(rule.Value);
    }

    private static TaxonRule CreateTaxonRule()
    {
        return TaxonRuleExtensions.Create(
            Guid.NewGuid(),
            TaxonRuleType.ProductName,
            TaxonRuleMatchPolicy.IsEqualTo,
            "T-Shirt",
            id: Guid.NewGuid());
    }
}
