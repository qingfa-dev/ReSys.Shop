using System.Globalization;

using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Domain.Taxonomies.Taxons.Rules;
using Module.Catalog.Features.Admin.Taxons.Services.AutoClassification.Abstractions;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Services.AutoClassification;

public abstract class TaxonRuleEvaluatorTestsBase
{
    protected abstract ITaxonRuleEvaluator CreateEvaluator();

    [Fact(DisplayName = "Evaluate: Should return false if taxon is not automatic")]
    public void Evaluate_TaxonNotAutomatic_ShouldReturnFalse()
    {
        var sut = CreateEvaluator();
        var product = CreateProduct("Test Product");
        var taxon = CreateTaxon("Test Taxon", automatic: false);
        taxon.TaxonRules.Add(TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductName, TaxonRuleMatchPolicy.IsEqualTo, "Test Product"));

        var result = sut.Evaluate(product, taxon);

        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Evaluate: Should return false if taxon has no rules")]
    public void Evaluate_NoRules_ShouldReturnFalse()
    {
        var sut = CreateEvaluator();
        var product = CreateProduct("Test Product");
        var taxon = CreateTaxon("Test Taxon", automatic: true);

        var result = sut.Evaluate(product, taxon);

        result.Should().BeFalse();
    }

    [Theory(DisplayName = "Evaluate: Should evaluate ProductName correctly")]
    [InlineData("Test Product", "Test", TaxonRuleMatchPolicy.Contains, true)]
    [InlineData("Test Product", "Test", TaxonRuleMatchPolicy.IsEqualTo, false)]
    [InlineData("Test Product", "Test Product", TaxonRuleMatchPolicy.IsEqualTo, true)]
    [InlineData("Test Product", "test product", TaxonRuleMatchPolicy.IsEqualTo, true)] // Case-insensitive
    [InlineData("Test Product", "Other", TaxonRuleMatchPolicy.IsNotEqualTo, true)]
    [InlineData("Test Product", "Test", TaxonRuleMatchPolicy.StartsWith, true)]
    [InlineData("Test Product", "Product", TaxonRuleMatchPolicy.EndsWith, true)]
    public void Evaluate_ProductNameRule_ShouldReturnExpected(string productName, string ruleValue, TaxonRuleMatchPolicy policy, bool expected)
    {
        var sut = CreateEvaluator();
        var product = CreateProduct(productName);
        var taxon = CreateTaxon("Taxon", automatic: true);
        taxon.TaxonRules.Add(TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductName, policy, ruleValue));

        var result = sut.Evaluate(product, taxon);

        result.Should().Be(expected);
    }

    [Theory(DisplayName = "Evaluate: Should evaluate IN/NOT IN correctly")]
    [InlineData("Apple", "Apple, Orange, Banana", TaxonRuleMatchPolicy.In, true)]
    [InlineData("Grapes", "Apple, Orange, Banana", TaxonRuleMatchPolicy.In, false)]
    [InlineData("Apple", "Apple, Orange, Banana", TaxonRuleMatchPolicy.NotIn, false)]
    [InlineData("Grapes", "Apple, Orange, Banana", TaxonRuleMatchPolicy.NotIn, true)]
    public void Evaluate_InNotIn_ShouldReturnExpected(string productName, string ruleValue, TaxonRuleMatchPolicy policy, bool expected)
    {
        var sut = CreateEvaluator();
        var product = CreateProduct(productName);
        var taxon = CreateTaxon("Taxon", automatic: true);
        taxon.TaxonRules.Add(TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductName, policy, ruleValue));

        var result = sut.Evaluate(product, taxon);

        result.Should().Be(expected);
    }

    [Theory(DisplayName = "Evaluate: Should evaluate Numeric rules correctly")]
    [InlineData(100.50, "100.50", TaxonRuleMatchPolicy.IsEqualTo, true)]
    [InlineData(100.50, "100", TaxonRuleMatchPolicy.GreaterThan, true)]
    [InlineData(100.50, "101", TaxonRuleMatchPolicy.LessThan, true)]
    [InlineData(null, "100", TaxonRuleMatchPolicy.IsEqualTo, false)]
    [InlineData(null, "100", TaxonRuleMatchPolicy.IsNull, true)]
    [InlineData(100.50, "100", TaxonRuleMatchPolicy.IsNotNull, true)]
    public void Evaluate_DecimalRules_ShouldReturnExpected(double? price, string ruleValue, TaxonRuleMatchPolicy policy, bool expected)
    {
        var sut = CreateEvaluator();
        var product = CreateProduct("Product");
        var master = VariantMethod.Create(product.Id, "SKU-1", isMaster: true).Value;
        master.Price = (decimal?)price;
        product.Variants.Add(master);

        var taxon = CreateTaxon("Taxon", automatic: true);
        taxon.TaxonRules.Add(TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductPrice, policy, ruleValue));

        var result = sut.Evaluate(product, taxon);

        result.Should().Be(expected);
    }

    [Theory(DisplayName = "Evaluate: Should evaluate Master Variant SKU correctly")]
    [InlineData("MASTER-SKU", "MASTER-SKU", true)]
    [InlineData("MASTER-SKU", "OTHER", false)]
    public void Evaluate_ProductSku_ShouldCheckMasterVariant(string actualSku, string ruleValue, bool expected)
    {
        var sut = CreateEvaluator();
        var product = CreateProduct("Product");
        var master = VariantMethod.Create(product.Id, actualSku, isMaster: true).Value;
        product.Variants.Add(master);
        var other = VariantMethod.Create(product.Id, "OTHER-SKU", isMaster: false).Value;
        product.Variants.Add(other);

        var taxon = CreateTaxon("Taxon", automatic: true);
        taxon.TaxonRules.Add(TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductSku, TaxonRuleMatchPolicy.IsEqualTo, ruleValue));

        var result = sut.Evaluate(product, taxon);

        result.Should().Be(expected);
    }

    [Theory(DisplayName = "Evaluate: Should evaluate ANY Variant SKU correctly")]
    [InlineData("VAR-2", "VAR-2", true)]
    [InlineData("VAR-2", "VAR-3", false)]
    public void Evaluate_VariantSku_ShouldCheckAnyVariant(string actualSku2, string ruleValue, bool expected)
    {
        var sut = CreateEvaluator();
        var product = CreateProduct("Product");
        product.Variants.Add(VariantMethod.Create(product.Id, "VAR-1", isMaster: true).Value);
        product.Variants.Add(VariantMethod.Create(product.Id, actualSku2, isMaster: false).Value);

        var taxon = CreateTaxon("Taxon", automatic: true);
        taxon.TaxonRules.Add(TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.VariantSku, TaxonRuleMatchPolicy.IsEqualTo, ruleValue));

        var result = sut.Evaluate(product, taxon);

        result.Should().Be(expected);
    }

    [Fact(DisplayName = "Evaluate: Should respect TaxonMatchPolicy.All (AND)")]
    public void Evaluate_MatchPolicyAll_ShouldRequireAllRulesToMatch()
    {
        var sut = CreateEvaluator();
        var product = CreateProduct("Blue T-Shirt");
        var taxon = CreateTaxon("Blue Shirts", automatic: true);
        taxon.RulesMatchPolicy = TaxonMatchPolicy.All;
        
        taxon.TaxonRules.Add(TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductName, TaxonRuleMatchPolicy.Contains, "Blue"));
        taxon.TaxonRules.Add(TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductName, TaxonRuleMatchPolicy.Contains, "Shirt"));
        taxon.TaxonRules.Add(TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductName, TaxonRuleMatchPolicy.Contains, "Red"));

        var result = sut.Evaluate(product, taxon);

        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Evaluate: Should respect TaxonMatchPolicy.Any (OR)")]
    public void Evaluate_MatchPolicyAny_ShouldMatchIfAnyRuleMatches()
    {
        var sut = CreateEvaluator();
        var product = CreateProduct("Blue T-Shirt");
        var taxon = CreateTaxon("Colorful Items", automatic: true);
        taxon.RulesMatchPolicy = TaxonMatchPolicy.Any;
        
        taxon.TaxonRules.Add(TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductName, TaxonRuleMatchPolicy.Contains, "Blue"));
        taxon.TaxonRules.Add(TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductName, TaxonRuleMatchPolicy.Contains, "Red"));

        var result = sut.Evaluate(product, taxon);

        result.Should().BeTrue();
    }

    [Theory(DisplayName = "Evaluate: Should evaluate Boolean status rules correctly")]
    [InlineData(ProductStatus.Active, "true", TaxonRuleType.ProductAvailable, true)]
    [InlineData(ProductStatus.Draft, "true", TaxonRuleType.ProductAvailable, false)]
    [InlineData(ProductStatus.Archived, "true", TaxonRuleType.ProductArchived, true)]
    [InlineData(ProductStatus.Active, "true", TaxonRuleType.ProductArchived, false)]
    [InlineData(ProductStatus.Active, "false", TaxonRuleType.ProductAvailable, false)]
    public void Evaluate_BooleanStatusRules_ShouldReturnExpected(ProductStatus status, string ruleValue, TaxonRuleType type, bool expected)
    {
        var sut = CreateEvaluator();
        var product = CreateProduct("Product");
        product.Status = status;
        var taxon = CreateTaxon("Taxon", automatic: true);
        taxon.TaxonRules.Add(TaxonRuleExtensions.Create(taxon.Id, type, TaxonRuleMatchPolicy.IsEqualTo, ruleValue));

        var result = sut.Evaluate(product, taxon);

        result.Should().Be(expected);
    }

    [Theory(DisplayName = "Evaluate: Should evaluate ProductStatus enum correctly")]
    [InlineData(ProductStatus.Active, "Active", TaxonRuleMatchPolicy.IsEqualTo, true)]
    [InlineData(ProductStatus.Draft, "Active", TaxonRuleMatchPolicy.IsEqualTo, false)]
    [InlineData(ProductStatus.Archived, "Active", TaxonRuleMatchPolicy.IsNotEqualTo, true)]
    public void Evaluate_ProductStatusEnumRule_ShouldReturnExpected(ProductStatus status, string ruleValue, TaxonRuleMatchPolicy policy, bool expected)
    {
        var sut = CreateEvaluator();
        var product = CreateProduct("Product");
        product.Status = status;
        var taxon = CreateTaxon("Taxon", automatic: true);
        taxon.TaxonRules.Add(TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductStatus, policy, ruleValue));

        var result = sut.Evaluate(product, taxon);

        result.Should().Be(expected);
    }

    [Theory(DisplayName = "Evaluate: Should evaluate advanced Decimal policies correctly")]
    [InlineData(100, "100", TaxonRuleMatchPolicy.GreaterThanOrEqual, true)]
    [InlineData(99.99, "100", TaxonRuleMatchPolicy.GreaterThanOrEqual, false)]
    [InlineData(100, "100", TaxonRuleMatchPolicy.LessThanOrEqual, true)]
    [InlineData(100.01, "100", TaxonRuleMatchPolicy.LessThanOrEqual, false)]
    public void Evaluate_AdvancedDecimalPolicies_ShouldReturnExpected(double price, string ruleValue, TaxonRuleMatchPolicy policy, bool expected)
    {
        var sut = CreateEvaluator();
        var product = CreateProduct("Product");
        var master = VariantMethod.Create(product.Id, "SKU-1", isMaster: true).Value;
        master.Price = (decimal)price;
        product.Variants.Add(master);

        var taxon = CreateTaxon("Taxon", automatic: true);
        taxon.TaxonRules.Add(TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductPrice, policy, ruleValue));

        var result = sut.Evaluate(product, taxon);

        result.Should().Be(expected);
    }

    [Theory(DisplayName = "Evaluate: Should handle String null/empty edge cases")]
    [InlineData(null, "", TaxonRuleMatchPolicy.IsEqualTo, true)]
    [InlineData("", "", TaxonRuleMatchPolicy.IsEqualTo, true)]
    [InlineData("something", "anything", TaxonRuleMatchPolicy.IsNotNull, true)]
    public void Evaluate_StringNullEmptyEdgeCases_ShouldReturnExpected(string? description, string ruleValue, TaxonRuleMatchPolicy policy, bool expected)
    {
        var sut = CreateEvaluator();
        var product = CreateProduct("Product");
        if (description != null) product.Description = description;
        var taxon = CreateTaxon("Taxon", automatic: true);
        taxon.TaxonRules.Add(TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductDescription, policy, ruleValue));

        var result = sut.Evaluate(product, taxon);

        result.Should().Be(expected);
    }

    [Theory(DisplayName = "Evaluate: Should evaluate ANY Variant Price correctly")]
    [InlineData(10, 20, "15", TaxonRuleMatchPolicy.GreaterThan, true)]
    [InlineData(10, 12, "15", TaxonRuleMatchPolicy.GreaterThan, false)]
    [InlineData(10, 15, "15", TaxonRuleMatchPolicy.IsEqualTo, true)]
    public void Evaluate_VariantPrice_ShouldCheckAnyVariant(double price1, double price2, string ruleValue, TaxonRuleMatchPolicy policy, bool expected)
    {
        var sut = CreateEvaluator();
        var product = CreateProduct("Product");
        var v1 = VariantMethod.Create(product.Id, "V1", isMaster: true).Value;
        v1.Price = (decimal)price1;
        product.Variants.Add(v1);
        
        var v2 = VariantMethod.Create(product.Id, "V2", isMaster: false).Value;
        v2.Price = (decimal)price2;
        product.Variants.Add(v2);

        var taxon = CreateTaxon("Taxon", automatic: true);
        taxon.TaxonRules.Add(TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.VariantPrice, policy, ruleValue));

        var result = sut.Evaluate(product, taxon);

        result.Should().Be(expected);
    }

    [Theory(DisplayName = "Evaluate: Should handle invalid parsing gracefully")]
    [InlineData("not-a-decimal", TaxonRuleType.ProductPrice, TaxonRuleMatchPolicy.GreaterThan, false)]
    [InlineData("not-a-bool", TaxonRuleType.ProductAvailable, TaxonRuleMatchPolicy.IsEqualTo, false)]
    [InlineData("InvalidStatus", TaxonRuleType.ProductStatus, TaxonRuleMatchPolicy.IsEqualTo, false)]
    public void Evaluate_InvalidParsing_ShouldReturnFalse(string ruleValue, TaxonRuleType type, TaxonRuleMatchPolicy policy, bool expected)
    {
        var sut = CreateEvaluator();
        var product = CreateProduct("Product");
        var master = VariantMethod.Create(product.Id, "SKU-1", isMaster: true).Value;
        master.Price = 100;
        product.Variants.Add(master);

        var taxon = CreateTaxon("Taxon", automatic: true);
        taxon.TaxonRules.Add(TaxonRuleExtensions.Create(taxon.Id, type, policy, ruleValue));

        var result = sut.Evaluate(product, taxon);

        result.Should().Be(expected);
    }

    protected static Product CreateProduct(string name)
    {
        return ProductMethod.Create(name: name, slug: name.ToLower(CultureInfo.InvariantCulture).Replace(" ", "-")).Value;
    }

    protected static Taxon CreateTaxon(string name, bool automatic)
    {
        return TaxonMethod.Create(
            taxonomyId: Guid.NewGuid(),
            parentId: null,
            name: name,
            presentation: name,
            description: name,
            position: 0,
            slug: name.ToLower(CultureInfo.InvariantCulture).Replace(" ", "-"),
            metaTitle: null,
            metaDescription: null,
            metaKeywords: null,
            automatic: automatic,
            rulesMatchPolicy: TaxonMatchPolicy.All,
            sortOrder: TaxonSortOrder.Manual,
            hideFromNav: false,
            imageUrl: null,
            squareImageUrl: null).Value;
    }
}
