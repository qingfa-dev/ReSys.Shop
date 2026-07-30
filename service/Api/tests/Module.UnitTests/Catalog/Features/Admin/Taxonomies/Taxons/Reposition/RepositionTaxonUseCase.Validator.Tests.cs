using Module.Catalog.Features.Admin.Taxonomies.Taxons.Reposition;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Reposition;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonReposition")]
public class RepositionTaxonValidatorTests
{
    private readonly RepositionTaxon.Validator _validator = new();

    [Fact(DisplayName = "Validator: Should pass for valid request")]
    public void Validator_ShouldPass_WhenValid()
    {
        var command = new RepositionTaxon.Command(
            Guid.NewGuid(), 
            new RepositionTaxon.Request { Position = 5 });

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Validator: Should fail when request is null")]
    public void Validator_ShouldFail_WhenRequestNull()
    {
        var command = new RepositionTaxon.Command(Guid.NewGuid(), null!);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Request);
    }

    [Theory(DisplayName = "Validator: Should fail for invalid position")]
    [InlineData(-2)]
    public void Validator_ShouldFail_WhenPositionInvalid(int position)
    {
        var command = new RepositionTaxon.Command(
            Guid.NewGuid(), 
            new RepositionTaxon.Request { Position = position });
        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Request.Position");
    }

    
}
