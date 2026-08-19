using Module.Catalog.Features.Admin.Products.ProductClassifications.Assign;
using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Classifications.Assign;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductClassificationAssign")]
public class AssignProductClassificationsValidatorTests
{
    private readonly AssignProductClassifications.Validator _validator = new();

    [Fact(DisplayName = "Validator: Should pass with valid request")]
    public void Validate_WithValidRequest_ShouldPass()
    {
        var command = new AssignProductClassifications.Command(
            Guid.NewGuid(),
            new AssignProductClassifications.Request { Items = [new ProductClassificationAssignmentItem { TaxonId = Guid.NewGuid(), Position = 0 }] });

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Validator: Should fail when Items is null")]
    public void Validate_WithNullItems_ShouldFail()
    {
        var command = new AssignProductClassifications.Command(
            Guid.NewGuid(),
            new AssignProductClassifications.Request { Items = null! });

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Request.Items);
    }

    [Fact(DisplayName = "Validator: Should pass when Items is empty (handler handles no-op)")]
    public void Validate_WithEmptyItems_ShouldPass()
    {
        var command = new AssignProductClassifications.Command(
            Guid.NewGuid(),
            new AssignProductClassifications.Request { Items = [] });

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
