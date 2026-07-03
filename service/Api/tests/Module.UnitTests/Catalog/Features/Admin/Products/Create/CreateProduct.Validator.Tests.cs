using Module.Catalog.Features.Admin.Products.Create;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Product")]
public class CreateProductValidatorTests
{
    private readonly CreateProduct.Validator _validator = new();

    [Fact(DisplayName = "Validator: Should fail when Request is null")]
    public void Validator_WhenRequestIsNull_ShouldHaveError()
    {
        var command = new CreateProduct.Command(null!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request);
    }

    [Fact(DisplayName = "Validator: Should fail when Name is empty")]
    public void Validator_WhenNameIsEmpty_ShouldHaveError()
    {
        var command = new CreateProduct.Command(new CreateProduct.Request { Name = "", Slug = "test", Description = "" });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.Name);
    }

    [Fact(DisplayName = "Validator: Should pass with valid request")]
    public void Validator_WhenValid_ShouldPass()
    {
        var command = new CreateProduct.Command(new CreateProduct.Request
        {
            Name = "T-Shirt",
            Slug = "t-shirt",
            Description = "A cotton t-shirt",
        });

        var result = _validator.TestValidate(command);

        result.IsValid.Should().BeTrue();
    }
}
