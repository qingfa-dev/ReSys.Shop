using Module.Catalog.Features.Admin.Products.Options.Sync;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Options.Sync;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductOptionTypeSync")]
public class SyncProductOptionTypesValidatorTests
{
    private readonly SyncProductOptionTypes.Validator _validator = new();

    [Fact(DisplayName = "Validator: Should pass with valid request")]
    public void Validate_WithValidRequest_ShouldPass()
    {
        var command = new SyncProductOptionTypes.Command(
            Guid.NewGuid(),
            new SyncProductOptionTypes.Request { Items = [new() { OptionTypeId = Guid.NewGuid(), Position = 1 }] });

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Validator: Should fail when Items is null")]
    public void Validate_WithNullItems_ShouldFail()
    {
        var command = new SyncProductOptionTypes.Command(
            Guid.NewGuid(),
            new SyncProductOptionTypes.Request { Items = null! });

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Request.Items);
    }

    [Fact(DisplayName = "Validator: Should pass when Items is empty (allowed for sync)")]
    public void Validate_WithEmptyItems_ShouldPass()
    {
        var command = new SyncProductOptionTypes.Command(
            Guid.NewGuid(),
            new SyncProductOptionTypes.Request { Items = [] });

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
