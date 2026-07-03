using Module.Catalog.Features.Admin.Products.Update;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Product")]
public class UpdateProductValidatorTests
{
    private readonly UpdateProduct.Validator _validator = new();

    [Fact(DisplayName = "Validator: Should fail when Request is null")]
    public void Validator_WhenRequestIsNull_ShouldHaveError()
    {
        var command = new UpdateProduct.Command(Guid.NewGuid(), null!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request);
    }

    [Fact(DisplayName = "Validator: Should fail when Name is empty")]
    public void Validator_WhenNameIsEmpty_ShouldHaveError()
    {
        var command = new UpdateProduct.Command(Guid.NewGuid(), new UpdateProduct.Request { Name = "", Slug = "test" });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.Name);
    }

    [Fact(DisplayName = "Validator: Should pass with valid request")]
    public void Validator_WhenValid_ShouldPass()
    {
        var command = new UpdateProduct.Command(Guid.NewGuid(), new UpdateProduct.Request
        {
            Name = "T-Shirt",
            Slug = "t-shirt",
        });

        var result = _validator.TestValidate(command);

        result.IsValid.Should().BeTrue();
    }
}
