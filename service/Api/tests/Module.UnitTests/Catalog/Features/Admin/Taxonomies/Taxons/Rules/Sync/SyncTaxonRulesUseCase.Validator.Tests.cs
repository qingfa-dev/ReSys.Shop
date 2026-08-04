using Module.Catalog.Domain.Taxonomies.Taxons.Rules;
using Module.Catalog.Features.Admin.Taxons.Rules.Sync;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Sync;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonRuleSync")]
public class SyncTaxonRulesValidatorTests
{
    private readonly SyncTaxonRules.Validator _validator = new();

    [Fact(DisplayName = "Validator: Should pass for valid request with rules")]
    public void Validator_ShouldPass_WhenValid()
    {
        var command = new SyncTaxonRules.Command(Guid.NewGuid(), new SyncTaxonRules.Request
        {
            Rules =
            [
                new SyncTaxonRules.SyncItem
                {
                    Type = "product_name",
                    MatchPolicy = "is_equal_to",
                    Value = "T-Shirt"
                }
            ]
        });

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Validator: Should fail when Rules is null")]
    public void Validator_ShouldFail_WhenRulesNull()
    {
        var command = new SyncTaxonRules.Command(Guid.NewGuid(), new SyncTaxonRules.Request
        {
            Rules = null!
        });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Request.Rules");
    }

    [Fact(DisplayName = "Validator: Should pass for empty Rules list")]
    public void Validator_ShouldPass_WhenRulesEmpty()
    {
        var command = new SyncTaxonRules.Command(Guid.NewGuid(), new SyncTaxonRules.Request
        {
            Rules = []
        });

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory(DisplayName = "Validator: Should fail when SyncItem has invalid Type")]
    [InlineData("")]
    [InlineData("invalid_type")]
    [InlineData(null)]
    public void Validator_ShouldFail_WhenTypeInvalid(string? type)
    {
        var command = new SyncTaxonRules.Command(Guid.NewGuid(), new SyncTaxonRules.Request
        {
            Rules =
            [
                new SyncTaxonRules.SyncItem
                {
                    Type = type!,
                    MatchPolicy = "is_equal_to",
                    Value = "test"
                }
            ]
        });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Request.Rules[0].Type");
    }

    [Theory(DisplayName = "Validator: Should fail when SyncItem has invalid MatchPolicy")]
    [InlineData("")]
    [InlineData("invalid_policy")]
    [InlineData(null)]
    public void Validator_ShouldFail_WhenMatchPolicyInvalid(string? policy)
    {
        var command = new SyncTaxonRules.Command(Guid.NewGuid(), new SyncTaxonRules.Request
        {
            Rules =
            [
                new SyncTaxonRules.SyncItem
                {
                    Type = "product_name",
                    MatchPolicy = policy!,
                    Value = "test"
                }
            ]
        });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Request.Rules[0].MatchPolicy");
    }

    [Theory(DisplayName = "Validator: Should fail when SyncItem has invalid Value")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validator_ShouldFail_WhenValueInvalid(string? value)
    {
        var command = new SyncTaxonRules.Command(Guid.NewGuid(), new SyncTaxonRules.Request
        {
            Rules =
            [
                new SyncTaxonRules.SyncItem
                {
                    Type = "product_name",
                    MatchPolicy = "is_equal_to",
                    Value = value!
                }
            ]
        });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Request.Rules[0].Value");
    }

    [Fact(DisplayName = "Validator: Should fail when SyncItem Value exceeds max length")]
    public void Validator_ShouldFail_WhenValueTooLong()
    {
        var longValue = new string('a', TaxonRuleConstant.Constraints.ValueMaxLength + 1);

        var command = new SyncTaxonRules.Command(Guid.NewGuid(), new SyncTaxonRules.Request
        {
            Rules =
            [
                new SyncTaxonRules.SyncItem
                {
                    Type = "product_name",
                    MatchPolicy = "is_equal_to",
                    Value = longValue
                }
            ]
        });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Request.Rules[0].Value");
    }
}
