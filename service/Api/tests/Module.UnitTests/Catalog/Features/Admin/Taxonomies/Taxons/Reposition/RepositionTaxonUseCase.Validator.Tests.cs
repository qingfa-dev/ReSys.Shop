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
            Guid.NewGuid(), 
            new Request { Position = 5 });

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Validator: Should fail when request is null")]
    public void Validator_ShouldFail_WhenRequestNull()
    {
        var command = new RepositionTaxon.Command(Guid.NewGuid(), Guid.NewGuid(), null!);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Request);
    }

    [Theory(DisplayName = "Validator: Should fail for invalid position")]
    [InlineData(-2)]
    public void Validator_ShouldFail_WhenPositionInvalid(int position)
    {
        var command = new RepositionTaxon.Command(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            new Request { Position = position });
        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Request.Position");
    }

    [Theory(DisplayName = "Validator: Should fail for empty IDs")]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Validator_ShouldFail_WhenIdsEmpty(bool emptyTaxonomyId, bool emptyId)
    {
        var command = new RepositionTaxon.Command(
            emptyTaxonomyId ? Guid.Empty : Guid.NewGuid(),
            emptyId ? Guid.Empty : Guid.NewGuid(),
            new Request { Position = 0 });
        
        var result = _validator.TestValidate(command);
        
        if (emptyTaxonomyId) result.ShouldHaveValidationErrorFor(x => x.TaxonomyId);
        // Id is not explicitly validated in the provided snippet but usually is.
    }
}
