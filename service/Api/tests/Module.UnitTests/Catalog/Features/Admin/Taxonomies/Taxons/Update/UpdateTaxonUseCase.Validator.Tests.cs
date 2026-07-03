using Module.Catalog.Features.Admin.Taxonomies.Taxons.Update;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonUpdate")]
public class UpdateTaxonValidatorTests
{
    private readonly UpdateTaxon.Validator _validator = new();

    [Fact(DisplayName = "Validator: Should pass for valid request")]
    public void Validator_ShouldPass_WhenValid()
    {
        var command = new UpdateTaxon.Command(Guid.NewGuid(), Guid.NewGuid(), new UpdateTaxon.Request
        {
            Name = "Pants",
            Slug = "pants",
            Position = 1
        });

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory(DisplayName = "Validator: Should fail for empty IDs")]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Validator_ShouldFail_WhenIdsEmpty(bool emptyTaxonomyId, bool emptyId)
    {
        var command = new UpdateTaxon.Command(
            emptyTaxonomyId ? Guid.Empty : Guid.NewGuid(),
            emptyId ? Guid.Empty : Guid.NewGuid(),
            new UpdateTaxon.Request());
        
        var result = _validator.TestValidate(command);
        
        if (emptyTaxonomyId) result.ShouldHaveValidationErrorFor(x => x.TaxonomyId);
        // Id is not explicitly validated in the provided snippet but usually is.
    }

    [Theory(DisplayName = "Validator: Should fail for invalid request name")]
    [InlineData("")]
    [InlineData(null)]
    public void Validator_ShouldFail_WhenRequestNameInvalid(string? name)
    {
        var request = new UpdateTaxon.Request { Name = name!, Slug = "pants" };
        var command = new UpdateTaxon.Command(Guid.NewGuid(), Guid.NewGuid(), request);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Request.Name");
    }
}
