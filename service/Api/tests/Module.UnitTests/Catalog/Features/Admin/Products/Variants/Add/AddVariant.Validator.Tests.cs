using Module.Catalog.Features.Admin.Products.Variants.Add;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Add;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Variant")]
public class AddVariantValidatorTests
{
    private readonly AddVariant.Validator _validator = new();

    [Fact(DisplayName = "Validator: Should fail when Request is null")]
    public void Validator_WhenRequestIsNull_ShouldHaveError()
    {
        var command = new AddVariant.Command(null!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request);
    }

    [Fact(DisplayName = "Validator: Should fail when Sku is empty")]
    public void Validator_WhenSkuIsEmpty_ShouldHaveError()
    {
        var command = new AddVariant.Command(new AddVariant.Request { Sku = "", ProductId = Guid.NewGuid() });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.Sku);
    }

    [Fact(DisplayName = "Validator: Should pass with valid request")]
    public void Validator_WhenValid_ShouldPass()
    {
        var command = new AddVariant.Command(new AddVariant.Request { Sku = "SKU-001", ProductId = Guid.NewGuid() });

        var result = _validator.TestValidate(command);

        result.IsValid.Should().BeTrue();
    }
}
