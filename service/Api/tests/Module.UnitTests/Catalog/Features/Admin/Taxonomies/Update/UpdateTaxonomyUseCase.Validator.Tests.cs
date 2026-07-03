using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Features.Admin.Taxonomies.Update;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonomyUpdate")]
public class UpdateTaxonomyValidatorTests
{
    private readonly UpdateTaxonomy.Validator _validator = new();

    [Fact(DisplayName = "Validator: Should pass for valid request")]
    public void Validator_ShouldPass_WhenValid()
    {
        var command = new UpdateTaxonomy.Command(Guid.NewGuid(), new UpdateTaxonomy.Request
        {
            Name = "New Name",
            Presentation = "New Presentation",
            Position = 1
        });

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Validator: Should fail when id is empty")]
    public void Validator_ShouldFail_WhenIdEmpty()
    {
        var command = new UpdateTaxonomy.Command(Guid.Empty, new UpdateTaxonomy.Request());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory(DisplayName = "Validator: Should fail when request name is invalid")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validator_ShouldFail_WhenNameIsInvalid(string? name)
    {
        var request = new UpdateTaxonomy.Request { Name = name!, Presentation = "Valid" };
        var command = new UpdateTaxonomy.Command(Guid.NewGuid(), request);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Request.Name");
    }

    [Theory(DisplayName = "Validator: Should fail when presentation is invalid")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validator_ShouldFail_WhenPresentationIsInvalid(string? presentation)
    {
        var request = new UpdateTaxonomy.Request { Name = "Valid", Presentation = presentation! };
        var command = new UpdateTaxonomy.Command(Guid.NewGuid(), request);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Request.Presentation");
    }

    [Fact(DisplayName = "Validator: Should fail when name exceeds max length")]
    public void Validator_ShouldFail_WhenNameTooLong()
    {
        var request = new UpdateTaxonomy.Request 
        { 
            Name = new string('a', TaxonomyConstant.Constraints.NameMaxLength + 1),
            Presentation = "Valid"
        };
        var command = new UpdateTaxonomy.Command(Guid.NewGuid(), request);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Request.Name");
    }
}
