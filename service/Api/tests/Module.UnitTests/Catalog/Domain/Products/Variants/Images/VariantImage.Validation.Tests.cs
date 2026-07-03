
using Module.Catalog.Domain.Products.Variants.Images;

namespace Module.UnitTests.Catalog.Domain.Products.Variants.Images;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "VariantImage")]
public class VariantImageValidationContentTypeTests
{
    private sealed class TestModel
    {
        public string? ContentType { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.ContentType).ApplyContentTypeRules();
        }
    }

    [Theory(DisplayName = "ContentType: Should fail when empty")]
    [InlineData("")]
    [InlineData(null)]
    public void ApplyContentTypeRules_WhenEmpty_ShouldHaveError(string? contentType)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { ContentType = contentType });

        result.ShouldHaveValidationErrorFor(x => x.ContentType);
    }

    [Fact(DisplayName = "ContentType: Should pass with valid content type")]
    public void ApplyContentTypeRules_WhenValid_ShouldNotHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { ContentType = "image/jpeg" });

        result.ShouldNotHaveValidationErrorFor(x => x.ContentType);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "VariantImage")]
public class VariantImageValidationUrlTests
{
    private sealed class TestModel
    {
        public string? Url { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.Url).ApplyUrlRules();
        }
    }

    [Theory(DisplayName = "Url: Should fail when empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ApplyUrlRules_WhenEmpty_ShouldHaveError(string? url)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Url = url });

        result.ShouldHaveValidationErrorFor(x => x.Url)
            .WithErrorCode(VariantImageResult.Failure.UrlRequired.Code);
    }

    [Theory(DisplayName = "Url: Should pass with valid url")]
    [InlineData("http://example.com/image.jpg")]
    [InlineData("https://cdn.shop.com/img.png")]
    public void ApplyUrlRules_WhenValid_ShouldNotHaveError(string url)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Url = url });

        result.ShouldNotHaveValidationErrorFor(x => x.Url);
    }
}
