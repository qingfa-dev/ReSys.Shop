using Module.Catalog.Features.Admin.Products.ProductClassifications.Revoke;
using Module.Catalog.Features.Admin.Products.ProductClassifications.Shared.Models;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Classifications.Revoke;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductClassificationRevoke")]
public class RevokeProductClassificationsValidatorTests
{
    private readonly RevokeProductClassifications.Validator _validator = new();

    [Fact(DisplayName = "Validator: Should pass with valid request")]
    public void Validate_WithValidRequest_ShouldPass()
    {
        var command = new RevokeProductClassifications.Command(
            Guid.NewGuid(),
            new RevokeProductClassifications.Request { Items = [new ProductClassificationAssignmentItem { TaxonId = Guid.NewGuid(), Position = 0 }] });

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Validator: Should fail when Items is null")]
    public void Validate_WithNullItems_ShouldFail()
    {
        var command = new RevokeProductClassifications.Command(
            Guid.NewGuid(),
            new RevokeProductClassifications.Request { Items = null! });

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Request.Items);
    }

    [Fact(DisplayName = "Validator: Should pass when Items is empty (handler handles no-op)")]
    public void Validate_WithEmptyItems_ShouldPass()
    {
        var command = new RevokeProductClassifications.Command(
            Guid.NewGuid(),
            new RevokeProductClassifications.Request { Items = [] });

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
