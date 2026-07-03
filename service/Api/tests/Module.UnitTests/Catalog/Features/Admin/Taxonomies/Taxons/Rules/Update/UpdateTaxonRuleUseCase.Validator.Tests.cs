using Module.Catalog.Domain.Taxonomies.Taxons.Rules;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Update;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonRuleUpdate")]
public class UpdateTaxonRuleValidatorTests
{
    private readonly UpdateTaxonRule.Validator _validator = new();

    [Fact(DisplayName = "Validator: Should pass for valid request")]
    public void Validator_ShouldPass_WhenValid()
    {
        var command = new UpdateTaxonRule.Command(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new UpdateTaxonRule.Request
        {
            Type = "product_name",
            MatchPolicy = "is_equal_to",
            Value = "T-Shirt"
        });

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Validator: Should fail when Request is null")]
    public void Validator_ShouldFail_WhenRequestNull()
    {
        var command = new UpdateTaxonRule.Command(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Request");
    }

    [Theory(DisplayName = "Validator: Should fail for invalid Type")]
    [InlineData("")]
    [InlineData("invalid_type")]
    [InlineData(null)]
    public void Validator_ShouldFail_WhenTypeInvalid(string? type)
    {
        var command = new UpdateTaxonRule.Command(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new UpdateTaxonRule.Request
        {
            Type = type!,
            MatchPolicy = "is_equal_to",
            Value = "test"
        });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Request.Type");
    }

    [Theory(DisplayName = "Validator: Should fail for invalid MatchPolicy")]
    [InlineData("")]
    [InlineData("invalid_policy")]
    [InlineData(null)]
    public void Validator_ShouldFail_WhenMatchPolicyInvalid(string? policy)
    {
        var command = new UpdateTaxonRule.Command(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new UpdateTaxonRule.Request
        {
            Type = "product_name",
            MatchPolicy = policy!,
            Value = "test"
        });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Request.MatchPolicy");
    }

    [Theory(DisplayName = "Validator: Should fail for invalid Value")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validator_ShouldFail_WhenValueInvalid(string? value)
    {
        var command = new UpdateTaxonRule.Command(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new UpdateTaxonRule.Request
        {
            Type = "product_name",
            MatchPolicy = "is_equal_to",
            Value = value!
        });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Request.Value");
    }

    [Fact(DisplayName = "Validator: Should fail when Value exceeds max length")]
    public void Validator_ShouldFail_WhenValueTooLong()
    {
        var longValue = new string('a', TaxonRuleConstant.Constraints.ValueMaxLength + 1);

        var command = new UpdateTaxonRule.Command(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new UpdateTaxonRule.Request
        {
            Type = "product_name",
            MatchPolicy = "is_equal_to",
            Value = longValue
        });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Request.Value");
    }

    [Fact(DisplayName = "Validator: Should fail when TaxonomyId is empty")]
    public void Validator_ShouldFail_WhenTaxonomyIdEmpty()
    {
        var command = new UpdateTaxonRule.Command(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), new UpdateTaxonRule.Request
        {
            Type = "product_name",
            MatchPolicy = "is_equal_to",
            Value = "test"
        });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TaxonomyId);
    }

    [Fact(DisplayName = "Validator: Should fail when TaxonId is empty")]
    public void Validator_ShouldFail_WhenTaxonIdEmpty()
    {
        var command = new UpdateTaxonRule.Command(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), new UpdateTaxonRule.Request
        {
            Type = "product_name",
            MatchPolicy = "is_equal_to",
            Value = "test"
        });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TaxonId);
    }

    [Fact(DisplayName = "Validator: Should fail when RuleId is empty")]
    public void Validator_ShouldFail_WhenRuleIdEmpty()
    {
        var command = new UpdateTaxonRule.Command(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, new UpdateTaxonRule.Request
        {
            Type = "product_name",
            MatchPolicy = "is_equal_to",
            Value = "test"
        });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.RuleId);
    }
}
