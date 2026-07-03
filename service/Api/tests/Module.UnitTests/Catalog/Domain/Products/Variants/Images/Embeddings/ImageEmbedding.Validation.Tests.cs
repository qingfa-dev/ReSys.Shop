using Module.Catalog.Domain.Products.Variants.Images.Embeddings;

namespace Module.UnitTests.Catalog.Domain.Products.VariantImageMethod.Embeddings;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "ImageEmbedding")]
public class ImageEmbeddingValidationModelNameTests
{
    private sealed class TestModel { public string? ModelName { get; set; } }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator() => RuleFor(x => x.ModelName).ApplyModelNameRules();
    }

    [Theory(DisplayName = "ModelName: Should fail when empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ApplyModelNameRules_WhenEmpty_ShouldHaveError(string? modelName)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { ModelName = modelName });

        result.ShouldHaveValidationErrorFor(x => x.ModelName)
            .WithErrorCode(ImageEmbeddingResult.Errors.ModelNameRequired.Code);
    }

    [Fact(DisplayName = "ModelName: Should fail when exceeds max length")]
    public void ApplyModelNameRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longName = new string('a', ImageEmbeddingConstant.Constraints.ModelNameMaxLength + 1);
        var result = validator.TestValidate(new TestModel { ModelName = longName });

        result.ShouldHaveValidationErrorFor(x => x.ModelName)
            .WithErrorCode(ImageEmbeddingResult.Errors.ModelNameTooLong.Code);
    }

    [Fact(DisplayName = "ModelName: Should pass with valid name")]
    public void ApplyModelNameRules_WhenValid_ShouldPass()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { ModelName = "ResNet-50" });

        result.ShouldNotHaveValidationErrorFor(x => x.ModelName);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "ImageEmbedding")]
public class ImageEmbeddingValidationModelVersionTests
{
    private sealed class TestModel { public string? ModelVersion { get; set; } }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator() => RuleFor(x => x.ModelVersion).ApplyModelVersionRules();
    }

    [Fact(DisplayName = "ModelVersion: Should fail when exceeds max length")]
    public void ApplyModelVersionRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longVersion = new string('a', ImageEmbeddingConstant.Constraints.ModelVersionMaxLength + 1);
        var result = validator.TestValidate(new TestModel { ModelVersion = longVersion });

        result.ShouldHaveValidationErrorFor(x => x.ModelVersion)
            .WithErrorCode(ImageEmbeddingResult.Errors.ModelVersionTooLong.Code);
    }

    [Fact(DisplayName = "ModelVersion: Should pass with valid version")]
    public void ApplyModelVersionRules_WhenValid_ShouldPass()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { ModelVersion = "v1.0.0" });

        result.ShouldNotHaveValidationErrorFor(x => x.ModelVersion);
    }
}
