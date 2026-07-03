using Module.Catalog.Features.Admin.Taxonomies.Taxons.Create;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonCreate")]
public class CreateTaxonValidatorTests
{
    private readonly CreateTaxon.Validator _validator = new();

    [Fact(DisplayName = "Validator: Should pass for valid request")]
    public void Validator_ShouldPass_WhenValid()
    {
        var command = new CreateTaxon.Command(Guid.NewGuid(), new CreateTaxon.Request
        {
            Name = "Shirts",
            Slug = "shirts",
            Position = 0
        });

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory(DisplayName = "Validator: Should fail for invalid names")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validator_ShouldFail_WhenNameInvalid(string? name)
    {
        var command = new CreateTaxon.Command(Guid.NewGuid(), new CreateTaxon.Request 
        { 
            Name = name!,
            Slug = "shirts"
        });
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Request.Name");
    }

    [Theory(DisplayName = "Validator: Should fail for invalid slug formats")]
    [InlineData("Invalid Slug")]
    [InlineData("slug_underscore")]
    [InlineData("-start-hyphen")]
    [InlineData("end-hyphen-")]
    [InlineData("Uppercase")]
    public void Validator_ShouldFail_WhenSlugInvalid(string slug)
    {
        var command = new CreateTaxon.Command(Guid.NewGuid(), new CreateTaxon.Request 
        { 
            Name = "Shirts",
            Slug = slug 
        });
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Request.Slug");
    }

    [Fact(DisplayName = "Validator: Should fail when TaxonomyId is empty")]
    public void Validator_ShouldFail_WhenTaxonomyIdEmpty()
    {
        var command = new CreateTaxon.Command(Guid.Empty, new CreateTaxon.Request());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TaxonomyId);
    }
}
