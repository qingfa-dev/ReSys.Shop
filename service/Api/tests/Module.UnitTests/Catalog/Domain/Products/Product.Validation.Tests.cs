using Module.Catalog.Domain.Products;

namespace Module.UnitTests.Catalog.Domain.Products;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Product")]
public class ProductValidationNameTests
{
    private sealed class TestModel
    {
        public string? Name { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.Name).ApplyNameRules();
        }
    }

    [Theory(DisplayName = "Name: Should fail when name is empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ApplyNameRules_WhenEmpty_ShouldHaveError(string? name)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Name = name });

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(ProductResult.Errors.NameRequired.Code);
    }

    [Fact(DisplayName = "Name: Should fail when name exceeds max length")]
    public void ApplyNameRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longName = new string('a', ProductConstant.Constraints.MaxNameLength + 1);
        var result = validator.TestValidate(new TestModel { Name = longName });

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(ProductResult.Errors.NameTooLong.Code);
    }

    [Fact(DisplayName = "Name: Should pass at exactly max length")]
    public void ApplyNameRules_WhenAtMaxLength_ShouldPass()
    {
        var validator = new TestValidator();
        var name = new string('a', ProductConstant.Constraints.MaxNameLength);
        var result = validator.TestValidate(new TestModel { Name = name });

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Theory(DisplayName = "Name: Should pass with valid name")]
    [InlineData("Premium T-Shirt")]
    [InlineData("X")]
    public void ApplyNameRules_WhenValid_ShouldPass(string name)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Name = name });

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Product")]
public class ProductValidationSlugTests
{
    private sealed class TestModel
    {
        public string? Slug { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.Slug).ApplySlugRules();
        }
    }

    [Theory(DisplayName = "Slug: Should fail when slug is empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ApplySlugRules_WhenEmpty_ShouldHaveError(string? slug)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Slug = slug });

        result.ShouldHaveValidationErrorFor(x => x.Slug)
            .WithErrorCode(ProductResult.Errors.SlugRequired.Code);
    }

    [Fact(DisplayName = "Slug: Should fail when slug exceeds max length")]
    public void ApplySlugRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longSlug = new string('a', ProductConstant.Constraints.MaxSlugLength + 1);
        var result = validator.TestValidate(new TestModel { Slug = longSlug });

        result.ShouldHaveValidationErrorFor(x => x.Slug)
            .WithErrorCode(ProductResult.Errors.SlugTooLong.Code);
    }

    [Fact(DisplayName = "Slug: Should pass at exactly max length")]
    public void ApplySlugRules_WhenAtMaxLength_ShouldPass()
    {
        var validator = new TestValidator();
        var slug = new string('a', ProductConstant.Constraints.MaxSlugLength);
        var result = validator.TestValidate(new TestModel { Slug = slug });

        result.ShouldNotHaveValidationErrorFor(x => x.Slug);
    }

    [Theory(DisplayName = "Slug: Should pass with valid slug")]
    [InlineData("premium-t-shirt")]
    [InlineData("x")]
    public void ApplySlugRules_WhenValid_ShouldPass(string slug)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Slug = slug });

        result.ShouldNotHaveValidationErrorFor(x => x.Slug);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Product")]
public class ProductValidationStatusTests
{
    private sealed class TestModel
    {
        public ProductStatus Status { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.Status).ApplyStatusRules();
        }
    }

    [Theory(DisplayName = "Status: Should fail when status is not in enum")]
    [InlineData((ProductStatus)999)]
    public void ApplyStatusRules_WhenInvalid_ShouldHaveError(ProductStatus status)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Status = status });

        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorCode(ProductResult.Errors.InvalidStatus.Code);
    }

    [Theory(DisplayName = "Status: Should pass with valid status")]
    [InlineData(ProductStatus.Draft)]
    [InlineData(ProductStatus.Archived)]
    public void ApplyStatusRules_WhenValid_ShouldPass(ProductStatus status)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Status = status });

        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Product")]
public class ProductValidationDescriptionTests
{
    private sealed class TestModel
    {
        public string? Description { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.Description).ApplyDescriptionRules();
        }
    }

    [Fact(DisplayName = "Description: Should fail when exceeds max length")]
    public void ApplyDescriptionRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longDescription = new string('a', ProductConstant.Constraints.MaxDescriptionLength + 1);
        var result = validator.TestValidate(new TestModel { Description = longDescription });

        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorCode(ProductResult.Errors.DescriptionTooLong.Code);
    }

    [Fact(DisplayName = "Description: Should pass at exactly max length")]
    public void ApplyDescriptionRules_WhenAtMaxLength_ShouldPass()
    {
        var validator = new TestValidator();
        var description = new string('a', ProductConstant.Constraints.MaxDescriptionLength);
        var result = validator.TestValidate(new TestModel { Description = description });

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Theory(DisplayName = "Description: Should pass with valid description")]
    [InlineData("A valid product description")]
    [InlineData("")]
    [InlineData(null)]
    public void ApplyDescriptionRules_WhenValid_ShouldPass(string? description)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Description = description });

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Product")]
public class ProductValidationMetaTitleTests
{
    private sealed class TestModel
    {
        public string? MetaTitle { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.MetaTitle).ApplyMetaTitleRules();
        }
    }

    [Fact(DisplayName = "MetaTitle: Should fail when exceeds max length")]
    public void ApplyMetaTitleRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longMetaTitle = new string('a', ProductConstant.Constraints.MaxMetaTitleLength + 1);
        var result = validator.TestValidate(new TestModel { MetaTitle = longMetaTitle });

        result.ShouldHaveValidationErrorFor(x => x.MetaTitle)
            .WithErrorCode(ProductResult.Errors.MetaTitleTooLong.Code);
    }

    [Fact(DisplayName = "MetaTitle: Should pass at exactly max length")]
    public void ApplyMetaTitleRules_WhenAtMaxLength_ShouldPass()
    {
        var validator = new TestValidator();
        var metaTitle = new string('a', ProductConstant.Constraints.MaxMetaTitleLength);
        var result = validator.TestValidate(new TestModel { MetaTitle = metaTitle });

        result.ShouldNotHaveValidationErrorFor(x => x.MetaTitle);
    }

    [Theory(DisplayName = "MetaTitle: Should pass with valid meta title")]
    [InlineData("SEO Title")]
    [InlineData("")]
    [InlineData(null)]
    public void ApplyMetaTitleRules_WhenValid_ShouldPass(string? metaTitle)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { MetaTitle = metaTitle });

        result.ShouldNotHaveValidationErrorFor(x => x.MetaTitle);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Product")]
public class ProductValidationMetaDescriptionTests
{
    private sealed class TestModel
    {
        public string? MetaDescription { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.MetaDescription).ApplyMetaDescriptionRules();
        }
    }

    [Fact(DisplayName = "MetaDescription: Should fail when exceeds max length")]
    public void ApplyMetaDescriptionRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longMetaDesc = new string('a', ProductConstant.Constraints.MaxMetaDescriptionLength + 1);
        var result = validator.TestValidate(new TestModel { MetaDescription = longMetaDesc });

        result.ShouldHaveValidationErrorFor(x => x.MetaDescription)
            .WithErrorCode(ProductResult.Errors.MetaDescriptionTooLong.Code);
    }

    [Fact(DisplayName = "MetaDescription: Should pass at exactly max length")]
    public void ApplyMetaDescriptionRules_WhenAtMaxLength_ShouldPass()
    {
        var validator = new TestValidator();
        var metaDesc = new string('a', ProductConstant.Constraints.MaxMetaDescriptionLength);
        var result = validator.TestValidate(new TestModel { MetaDescription = metaDesc });

        result.ShouldNotHaveValidationErrorFor(x => x.MetaDescription);
    }

    [Theory(DisplayName = "MetaDescription: Should pass with valid meta description")]
    [InlineData("A meta description for SEO")]
    [InlineData("")]
    [InlineData(null)]
    public void ApplyMetaDescriptionRules_WhenValid_ShouldPass(string? metaDescription)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { MetaDescription = metaDescription });

        result.ShouldNotHaveValidationErrorFor(x => x.MetaDescription);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Product")]
public class ProductValidationMetaKeywordsTests
{
    private sealed class TestModel
    {
        public string? MetaKeywords { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.MetaKeywords).ApplyMetaKeywordsRules();
        }
    }

    [Fact(DisplayName = "MetaKeywords: Should fail when exceeds max length")]
    public void ApplyMetaKeywordsRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longKeywords = new string('a', ProductConstant.Constraints.MaxMetaKeywordsLength + 1);
        var result = validator.TestValidate(new TestModel { MetaKeywords = longKeywords });

        result.ShouldHaveValidationErrorFor(x => x.MetaKeywords)
            .WithErrorCode(ProductResult.Errors.MetaKeywordsTooLong.Code);
    }

    [Fact(DisplayName = "MetaKeywords: Should pass at exactly max length")]
    public void ApplyMetaKeywordsRules_WhenAtMaxLength_ShouldPass()
    {
        var validator = new TestValidator();
        var keywords = new string('a', ProductConstant.Constraints.MaxMetaKeywordsLength);
        var result = validator.TestValidate(new TestModel { MetaKeywords = keywords });

        result.ShouldNotHaveValidationErrorFor(x => x.MetaKeywords);
    }

    [Theory(DisplayName = "MetaKeywords: Should pass with valid meta keywords")]
    [InlineData("t-shirt, cotton, premium")]
    [InlineData("")]
    [InlineData(null)]
    public void ApplyMetaKeywordsRules_WhenValid_ShouldPass(string? metaKeywords)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { MetaKeywords = metaKeywords });

        result.ShouldNotHaveValidationErrorFor(x => x.MetaKeywords);
    }
}
