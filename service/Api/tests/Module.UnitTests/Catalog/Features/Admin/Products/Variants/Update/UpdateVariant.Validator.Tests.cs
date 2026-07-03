using Module.Catalog.Features.Admin.Products.Variants.Update;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Variant")]
public class UpdateVariantValidatorTests
{
    private readonly UpdateVariant.Validator _validator = new();

    [Fact(DisplayName = "Validator: Should fail when Request is null")]
    public void Validator_WhenRequestIsNull_ShouldHaveError()
    {
        var command = new UpdateVariant.Command(Guid.NewGuid(), null!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request);
    }

    [Fact(DisplayName = "Validator: Should fail when Sku is empty")]
    public void Validator_WhenSkuIsEmpty_ShouldHaveError()
    {
        var command = new UpdateVariant.Command(Guid.NewGuid(), new UpdateVariant.Request { Sku = "" });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.Sku);
    }

    [Fact(DisplayName = "Validator: Should pass with valid request")]
    public void Validator_WhenValid_ShouldPass()
    {
        var command = new UpdateVariant.Command(Guid.NewGuid(), new UpdateVariant.Request { Sku = "SKU-001" });

        var result = _validator.TestValidate(command);

        result.IsValid.Should().BeTrue();
    }
}
