using Module.Catalog.Domain.Taxonomies.Taxons.Rules;

namespace Module.UnitTests.Catalog.Domain.Taxonomies.Taxons.Rules;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "TaxonRule")]
public class TaxonRuleExtensionsTests
{
    [Theory(DisplayName = "Create: Should return TaxonRule with correct properties")]
    [InlineData(TaxonRuleType.ProductName, TaxonRuleMatchPolicy.Contains, "Shirt")]
    [InlineData(TaxonRuleType.ProductPrice, TaxonRuleMatchPolicy.GreaterThan, "100")]
    public void Create_WithValidParameters_ShouldReturnTaxonRule(
        TaxonRuleType type,
        TaxonRuleMatchPolicy matchPolicy,
        string value)
    {
        var taxonId = Guid.NewGuid();
        var id = Guid.NewGuid();

        var rule = TaxonRuleExtensions.Create(taxonId, type, matchPolicy, value, id);

        rule.Id.Should().Be(id);
        rule.TaxonId.Should().Be(taxonId);
        rule.Type.Should().Be(type);
        rule.MatchPolicy.Should().Be(matchPolicy);
        rule.Value.Should().Be(value);
    }

    [Fact(DisplayName = "Update: Should update properties")]
    public void Update_WithNewValues_ShouldUpdateCorrectly()
    {
        var rule = TaxonRuleExtensions.Create(Guid.NewGuid(), TaxonRuleType.ProductName, TaxonRuleMatchPolicy.IsEqualTo, "Old");
        var newType = TaxonRuleType.ProductSku;
        var newPolicy = TaxonRuleMatchPolicy.StartsWith;
        var newValue = "New";

        var result = rule.Update(newType, newPolicy, newValue);

        result.IsSuccess.Should().BeTrue();
        rule.Type.Should().Be(newType);
        rule.MatchPolicy.Should().Be(newPolicy);
        rule.Value.Should().Be(newValue);
    }

    [Fact(DisplayName = "Update: Partial update with only type should preserve others")]
    public void Update_WithOnlyType_ShouldPreserveOthers()
    {
        var rule = TaxonRuleExtensions.Create(Guid.NewGuid(), TaxonRuleType.ProductName, TaxonRuleMatchPolicy.IsEqualTo, "Old Value");

        var result = rule.Update(type: TaxonRuleType.ProductSku);

        result.IsSuccess.Should().BeTrue();
        rule.Type.Should().Be(TaxonRuleType.ProductSku);
        rule.MatchPolicy.Should().Be(TaxonRuleMatchPolicy.IsEqualTo);
        rule.Value.Should().Be("Old Value");
    }
}
