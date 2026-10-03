using Module.Catalog.Features.Admin.Taxons.Create;

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
        var command = new CreateTaxon.Command(new CreateTaxon.Request
        {
            Name = "Shirts",
            Slug = "shirts",
            Position = 0,
            TaxonomyId = Guid.NewGuid(),
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
        var command = new CreateTaxon.Command(new CreateTaxon.Request 
        { 
            Name = name!,
            Slug = "shirts",
            TaxonomyId = Guid.NewGuid(),
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
        var command = new CreateTaxon.Command( new CreateTaxon.Request 
        { 
            Name = "Shirts",
            Slug = slug,
            TaxonomyId = Guid.NewGuid(),
        });
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Request.Slug");
    }

    [Fact(DisplayName = "Validator: Should fail when TaxonomyId is empty")]
    public void Validator_ShouldFail_WhenTaxonomyIdEmpty()
    {
        var command = new CreateTaxon.Command(new CreateTaxon.Request());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Request.TaxonomyId);
    }
}
