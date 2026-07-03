using CatalogDomain = Module.Catalog.Domain.Products;
using CatalogFeatureSharedModels = Module.Catalog.Features.Admin.Products.Shared.Models;
using CatalogFeatureValidation = Module.Catalog.Features.Admin.Products.Shared.Validation;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Shared.Validation;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Product")]
public class ProductParametersValidatorTests
{
    private readonly CatalogFeatureValidation.ProductValidation.ProductParametersValidator _validator = new();

    [Fact(DisplayName = "Validator: Should fail when Name is empty")]
    public void ProductParametersValidator_WhenNameIsEmpty_ShouldHaveError()
    {
        var model = new TestProductParameters { Name = "" };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(CatalogDomain.ProductResult.Errors.NameRequired.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when Name exceeds max length")]
    public void ProductParametersValidator_WhenNameIsTooLong_ShouldHaveError()
    {
        var model = new TestProductParameters { Name = new string('a', CatalogDomain.ProductConstant.Constraints.MaxNameLength + 1) };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(CatalogDomain.ProductResult.Errors.NameTooLong.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when Slug is empty")]
    public void ProductParametersValidator_WhenSlugIsEmpty_ShouldHaveError()
    {
        var model = new TestProductParameters { Slug = "" };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Slug)
            .WithErrorCode(CatalogDomain.ProductResult.Errors.SlugRequired.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when Slug exceeds max length")]
    public void ProductParametersValidator_WhenSlugIsTooLong_ShouldHaveError()
    {
        var model = new TestProductParameters { Slug = new string('a', CatalogDomain.ProductConstant.Constraints.MaxSlugLength + 1) };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Slug)
            .WithErrorCode(CatalogDomain.ProductResult.Errors.SlugTooLong.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when Description exceeds max length")]
    public void ProductParametersValidator_WhenDescriptionIsTooLong_ShouldHaveError()
    {
        var model = new TestProductParameters { Description = new string('a', CatalogDomain.ProductConstant.Constraints.MaxDescriptionLength + 1) };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorCode(CatalogDomain.ProductResult.Errors.DescriptionTooLong.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when MetaTitle exceeds max length")]
    public void ProductParametersValidator_WhenMetaTitleIsTooLong_ShouldHaveError()
    {
        var model = new TestProductParameters { MetaTitle = new string('a', CatalogDomain.ProductConstant.Constraints.MaxMetaTitleLength + 1) };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.MetaTitle)
            .WithErrorCode(CatalogDomain.ProductResult.Errors.MetaTitleTooLong.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when MetaDescription exceeds max length")]
    public void ProductParametersValidator_WhenMetaDescriptionIsTooLong_ShouldHaveError()
    {
        var model = new TestProductParameters { MetaDescription = new string('a', CatalogDomain.ProductConstant.Constraints.MaxMetaDescriptionLength + 1) };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.MetaDescription)
            .WithErrorCode(CatalogDomain.ProductResult.Errors.MetaDescriptionTooLong.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when MetaKeywords exceeds max length")]
    public void ProductParametersValidator_WhenMetaKeywordsIsTooLong_ShouldHaveError()
    {
        var model = new TestProductParameters { MetaKeywords = new string('a', CatalogDomain.ProductConstant.Constraints.MaxMetaKeywordsLength + 1) };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.MetaKeywords)
            .WithErrorCode(CatalogDomain.ProductResult.Errors.MetaKeywordsTooLong.Code);
    }

    [Theory(DisplayName = "Validator: Should pass with valid parameters")]
    [InlineData("T-Shirt", "t-shirt")]
    public void ProductParametersValidator_WhenValid_ShouldNotHaveErrors(string name, string slug)
    {
        var model = new TestProductParameters
        {
            Name = name,
            Slug = slug,
            Description = "A valid description",
            MetaTitle = "Valid Title",
            MetaDescription = "Valid meta description",
            MetaKeywords = "valid, keywords",
        };

        var result = _validator.TestValidate(model);

        result.IsValid.Should().BeTrue();
    }

    private sealed record TestProductParameters : CatalogFeatureSharedModels.ProductParameters;
}
