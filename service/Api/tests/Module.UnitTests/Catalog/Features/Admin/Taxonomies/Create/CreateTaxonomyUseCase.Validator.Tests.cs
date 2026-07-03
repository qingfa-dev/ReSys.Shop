using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Features.Admin.Taxonomies.Create;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonomyCreate")]
public class CreateTaxonomyValidatorTests
{
    private readonly CreateTaxonomy.Validator _validator = new();

    [Fact(DisplayName = "Validator: Should pass for valid request")]
    public void Validator_ShouldPass_WhenValid()
    {
        var command = new CreateTaxonomy.Command(new CreateTaxonomy.Request
        {
            Name = "Categories",
            Presentation = "Categories",
            Position = 0
        });

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Validator: Should fail when request is null")]
    public void Validator_ShouldFail_WhenRequestNull()
    {
        var command = new CreateTaxonomy.Command(null!);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Request);
    }

    [Theory(DisplayName = "Validator: Should fail when name is invalid")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validator_ShouldFail_WhenNameIsInvalid(string? name)
    {
        var request = new CreateTaxonomy.Request { Name = name!, Presentation = "Valid" };
        var command = new CreateTaxonomy.Command(request);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Request.Name");
    }

    [Fact(DisplayName = "Validator: Should fail when name exceeds max length")]
    public void Validator_ShouldFail_WhenNameTooLong()
    {
        var request = new CreateTaxonomy.Request 
        { 
            Name = new string('a', TaxonomyConstant.Constraints.NameMaxLength + 1),
            Presentation = "Valid"
        };
        var command = new CreateTaxonomy.Command(request);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Request.Name");
    }

    [Theory(DisplayName = "Validator: Should fail when presentation is invalid")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validator_ShouldFail_WhenPresentationIsInvalid(string? presentation)
    {
        var request = new CreateTaxonomy.Request { Name = "Valid", Presentation = presentation! };
        var command = new CreateTaxonomy.Command(request);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Request.Presentation");
    }

    [Theory(DisplayName = "Validator: Should fail when position is invalid")]
    [InlineData(-2)]
    public void Validator_ShouldFail_WhenPositionIsInvalid(int position)
    {
        var request = new CreateTaxonomy.Request { Name = "Valid", Presentation = "Valid", Position = position };
        var command = new CreateTaxonomy.Command(request);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Request.Position");
    }
}
