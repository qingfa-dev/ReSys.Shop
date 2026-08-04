using Module.Catalog.Features.Admin.Products.ProductClassifications.Shared.Models;
using Module.Catalog.Features.Admin.Products.ProductClassifications.Sync;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Classifications.Sync;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductClassificationSync")]
public class SyncProductClassificationsValidatorTests
{
    private readonly SyncProductClassifications.Validator _validator = new();

    [Fact(DisplayName = "Validator: Should pass with valid request")]
    public void Validate_WithValidRequest_ShouldPass()
    {
        var command = new SyncProductClassifications.Command(
            Guid.NewGuid(),
            new SyncProductClassifications.Request { Items = [new ProductClassificationAssignmentItem { TaxonId = Guid.NewGuid(), Position = 0 }] });

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Validator: Should fail when Items is null")]
    public void Validate_WithNullItems_ShouldFail()
    {
        var command = new SyncProductClassifications.Command(
            Guid.NewGuid(),
            new SyncProductClassifications.Request { Items = null! });

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Request.Items);
    }

    [Fact(DisplayName = "Validator: Should pass when Items is empty (allowed for sync)")]
    public void Validate_WithEmptyItems_ShouldPass()
    {
        var command = new SyncProductClassifications.Command(
            Guid.NewGuid(),
            new SyncProductClassifications.Request { Items = [] });

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
