using Module.Catalog.Features.Admin.Products.Options.Revoke;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Options.Revoke;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductOptionTypeRevoke")]
public class RevokeProductOptionTypesValidatorTests
{
    private readonly RevokeProductOptionTypes.Validator _validator = new();

    [Fact(DisplayName = "Validator: Should pass with valid request")]
    public void Validate_WithValidRequest_ShouldPass()
    {
        var command = new RevokeProductOptionTypes.Command(
            Guid.NewGuid(),
            new RevokeProductOptionTypes.Request { Items = [new() { OptionTypeId = Guid.NewGuid() }] });

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Validator: Should fail when Items is null")]
    public void Validate_WithNullItems_ShouldFail()
    {
        var command = new RevokeProductOptionTypes.Command(
            Guid.NewGuid(),
            new RevokeProductOptionTypes.Request { Items = null! });

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Request.Items);
    }

    [Fact(DisplayName = "Validator: Should pass when Items is empty (handler handles no-op)")]
    public void Validate_WithEmptyItems_ShouldPass()
    {
        var command = new RevokeProductOptionTypes.Command(
            Guid.NewGuid(),
            new RevokeProductOptionTypes.Request { Items = [] });

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
