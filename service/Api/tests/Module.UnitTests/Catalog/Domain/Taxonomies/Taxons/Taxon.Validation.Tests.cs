using Module.Catalog.Domain.Taxonomies.Taxons;

namespace Module.UnitTests.Catalog.Domain.Taxonomies.Taxons;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Taxon")]
public class TaxonValidationDescriptionTests
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

    [Fact(DisplayName = "Description: Should fail when Description exceeds max length")]
    public void ApplyDescriptionRules_WhenTooLong_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longDesc = new string('a', TaxonConstant.Constraints.DescriptionMaxLength + 1);
        var result = validator.TestValidate(new TestModel { Description = longDesc });

        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorCode(TaxonResult.Errors.DescriptionTooLong.Code);
    }

    [Theory(DisplayName = "Description: Should pass when Description is valid or null")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Valid Description")]
    public void ApplyDescriptionRules_WhenValid_ShouldNotHaveError(string? description)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Description = description });

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Taxon")]
public class TaxonValidationImageUrlTests
{
    private sealed class TestModel
    {
        public string? ImageUrl { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.ImageUrl).ApplyImageUrlRules();
        }
    }

    [Fact(DisplayName = "ImageUrl: Should fail when ImageUrl exceeds max length")]
    public void ApplyImageUrlRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longUrl = "http://example.com/" + new string('a', TaxonConstant.Constraints.UrlMaxLength);
        var result = validator.TestValidate(new TestModel { ImageUrl = longUrl });

        result.ShouldHaveValidationErrorFor(x => x.ImageUrl)
            .WithErrorCode(TaxonResult.Errors.ImageUrlTooLong.Code);
    }

    [Theory(DisplayName = "ImageUrl: Should fail when ImageUrl format is invalid")]
    [InlineData("not-a-url")]
    [InlineData("http://")]
    [InlineData("invalid-protocol://example.com")]
    [InlineData("")]
    public void ApplyImageUrlRules_WhenInvalidFormat_ShouldHaveError(string url)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { ImageUrl = url });

        result.ShouldHaveValidationErrorFor(x => x.ImageUrl)
            .WithErrorCode(TaxonResult.Errors.ImageUrlInvalidFormat.Code);
    }

    [Theory(DisplayName = "ImageUrl: Should pass when ImageUrl is valid")]
    [InlineData(null)]
    [InlineData("http://example.com/image.png")]
    [InlineData("https://cdn.shop.com/assets/cat.jpg")]
    public void ApplyImageUrlRules_WhenValid_ShouldNotHaveError(string? url)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { ImageUrl = url });

        result.ShouldNotHaveValidationErrorFor(x => x.ImageUrl);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Taxon")]
public class TaxonValidationMetaTests
{
    private sealed class TestModel
    {
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.MetaTitle).ApplyMetaTitleRules();
            RuleFor(x => x.MetaDescription).ApplyMetaDescriptionRules();
            RuleFor(x => x.MetaKeywords).ApplyMetaKeywordsRules();
        }
    }

    [Fact(DisplayName = "Meta: Should fail when metadata exceeds max length")]
    public void ApplyMetaRules_WhenTooLong_ShouldHaveError()
    {
        var validator = new TestValidator();
        var model = new TestModel
        {
            MetaTitle = new string('a', TaxonConstant.Constraints.MetaTitleMaxLength + 1),
            MetaDescription = new string('a', TaxonConstant.Constraints.MetaDescriptionMaxLength + 1),
            MetaKeywords = new string('a', TaxonConstant.Constraints.MetaKeywordsMaxLength + 1)
        };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.MetaTitle).WithErrorCode(TaxonResult.Errors.MetaTitleTooLong.Code);
        result.ShouldHaveValidationErrorFor(x => x.MetaDescription).WithErrorCode(TaxonResult.Errors.MetaDescriptionTooLong.Code);
        result.ShouldHaveValidationErrorFor(x => x.MetaKeywords).WithErrorCode(TaxonResult.Errors.MetaKeywordsTooLong.Code);
    }

    [Fact(DisplayName = "Meta: Should pass when metadata is valid or null")]
    public void ApplyMetaRules_WhenValid_ShouldNotHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel());

        result.ShouldNotHaveValidationErrorFor(x => x.MetaTitle);
        result.ShouldNotHaveValidationErrorFor(x => x.MetaDescription);
        result.ShouldNotHaveValidationErrorFor(x => x.MetaKeywords);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Taxon")]
public class TaxonValidationNameTests
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
            .WithErrorCode(TaxonResult.Errors.NameRequired.Code);
    }

    [Fact(DisplayName = "Name: Should fail when name exceeds max length")]
    public void ApplyNameRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longName = new string('a', TaxonConstant.Constraints.NameMaxLength + 1);
        var result = validator.TestValidate(new TestModel { Name = longName });

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(TaxonResult.Errors.NameTooLong.Code);
    }

    [Fact(DisplayName = "Name: Should pass at exactly max length")]
    public void ApplyNameRules_WhenAtMaxLength_ShouldNotHaveError()
    {
        var validator = new TestValidator();
        var name = new string('a', TaxonConstant.Constraints.NameMaxLength);
        var result = validator.TestValidate(new TestModel { Name = name });

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Theory(DisplayName = "Name: Should pass with valid name")]
    [InlineData("Valid Name")]
    [InlineData("Another Name")]
    public void ApplyNameRules_WhenValid_ShouldNotHaveError(string name)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Name = name });

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Taxon")]
public class TaxonValidationParentIdTests
{
    private sealed class TestModel
    {
        public Guid? ParentId { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.ParentId).ApplyTaxonParentIdRules();
        }
    }

    [Fact(DisplayName = "ParentId: Should fail when ParentId is empty Guid")]
    public void ApplyParentIdRules_WhenEmpty_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { ParentId = Guid.Empty });

        result.ShouldHaveValidationErrorFor(x => x.ParentId)
            .WithErrorCode(TaxonResult.Errors.InvalidParentId.Code);
    }

    [Theory(DisplayName = "ParentId: Should pass when ParentId is null or valid")]
    [InlineData(null)]
    public void ApplyParentIdRules_WhenValid_ShouldNotHaveError(Guid? ParentId)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { ParentId = ParentId });

        result.ShouldNotHaveValidationErrorFor(x => x.ParentId);
    }

    [Fact(DisplayName = "ParentId: Should pass when ParentId is valid Guid")]
    public void ApplyParentIdRules_WhenValidGuid_ShouldNotHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { ParentId = Guid.NewGuid() });

        result.ShouldNotHaveValidationErrorFor(x => x.ParentId);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Taxon")]
public class TaxonValidationPositionTests
{
    private sealed class TestModel
    {
        public int Position { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.Position).ApplyPositionRules();
        }
    }

    [Theory(DisplayName = "Position: Should fail when position is less than minimum")]
    [InlineData(-2)]
    public void ApplyPositionRules_WhenInvalid_ShouldHaveError(int position)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Position = position });

        result.ShouldHaveValidationErrorFor(x => x.Position)
            .WithErrorCode(TaxonResult.Errors.InvalidPosition.Code);
    }

    [Theory(DisplayName = "Position: Should pass when position is valid")]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(10)]
    public void ApplyPositionRules_WhenValid_ShouldNotHaveError(int position)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Position = position });

        result.ShouldNotHaveValidationErrorFor(x => x.Position);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Taxon")]
public class TaxonValidationSettingsTests
{
    private sealed class TestModel
    {
        public TaxonMatchPolicy RulesMatchPolicy { get; set; }
        public TaxonSortOrder SortOrder { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.RulesMatchPolicy).ApplyRulesMatchPolicyRules();
            RuleFor(x => x.SortOrder).ApplySortOrderRules();
        }
    }

    [Theory(DisplayName = "Settings: Should fail when RulesMatchPolicy is invalid")]
    [InlineData((TaxonMatchPolicy)999)]
    public void ApplyRulesMatchPolicyRules_WhenInvalid_ShouldHaveError(TaxonMatchPolicy policy)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { RulesMatchPolicy = policy });

        result.ShouldHaveValidationErrorFor(x => x.RulesMatchPolicy)
            .WithErrorCode(TaxonResult.Errors.InvalidRulesMatchPolicy.Code);
    }

    [Theory(DisplayName = "Settings: Should fail when SortOrder is invalid")]
    [InlineData((TaxonSortOrder)999)]
    public void ApplySortOrderRules_WhenInvalid_ShouldHaveError(TaxonSortOrder order)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { SortOrder = order });

        result.ShouldHaveValidationErrorFor(x => x.SortOrder)
            .WithErrorCode(TaxonResult.Errors.InvalidSortOrder.Code);
    }

    [Fact(DisplayName = "Settings: Should pass when values are valid")]
    public void ApplySettingsRules_WhenValid_ShouldNotHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel 
        { 
            RulesMatchPolicy = TaxonMatchPolicy.All,
            SortOrder = TaxonSortOrder.Newest
        });

        result.ShouldNotHaveValidationErrorFor(x => x.RulesMatchPolicy);
        result.ShouldNotHaveValidationErrorFor(x => x.SortOrder);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Taxon")]
public class TaxonValidationSlugTests
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
            .WithErrorCode(TaxonResult.Errors.SlugRequired.Code);
    }

    [Fact(DisplayName = "Slug: Should fail when slug exceeds max length")]
    public void ApplySlugRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longSlug = new string('a', TaxonConstant.Constraints.SlugMaxLength + 1);
        var result = validator.TestValidate(new TestModel { Slug = longSlug });

        result.ShouldHaveValidationErrorFor(x => x.Slug)
            .WithErrorCode(TaxonResult.Errors.SlugTooLong.Code);
    }

    [Theory(DisplayName = "Slug: Should fail when slug format is invalid")]
    [InlineData("Invalid Slug")]
    [InlineData("slug_with_underscore")]
    [InlineData("-slug-start-hyphen")]
    [InlineData("slug-end-hyphen-")]
    [InlineData("SlugWithUppercase")]
    public void ApplySlugRules_WhenInvalidFormat_ShouldHaveError(string slug)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Slug = slug });

        result.ShouldHaveValidationErrorFor(x => x.Slug)
            .WithErrorCode(TaxonResult.Errors.SlugInvalidFormat.Code);
    }

    [Theory(DisplayName = "Slug: Should pass when slug is valid")]
    [InlineData("valid-slug")]
    [InlineData("slug123")]
    [InlineData("a-b-c")]
    public void ApplySlugRules_WhenValid_ShouldNotHaveError(string slug)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Slug = slug });

        result.ShouldNotHaveValidationErrorFor(x => x.Slug);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Taxon")]
public class TaxonValidationSquareImageUrlTests
{
    private sealed class TestModel
    {
        public string? SquareImageUrl { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.SquareImageUrl).ApplySquareImageUrlRules();
        }
    }

    [Fact(DisplayName = "SquareImageUrl: Should fail when SquareImageUrl exceeds max length")]
    public void ApplySquareImageUrlRules_WhenTooLong_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longUrl = "http://example.com/" + new string('a', TaxonConstant.Constraints.UrlMaxLength);
        var result = validator.TestValidate(new TestModel { SquareImageUrl = longUrl });

        result.ShouldHaveValidationErrorFor(x => x.SquareImageUrl)
            .WithErrorCode(TaxonResult.Errors.SquareImageUrlTooLong.Code);
    }

    [Theory(DisplayName = "SquareImageUrl: Should fail when format is invalid")]
    [InlineData("invalid")]
    public void ApplySquareImageUrlRules_WhenInvalidFormat_ShouldHaveError(string url)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { SquareImageUrl = url });

        result.ShouldHaveValidationErrorFor(x => x.SquareImageUrl)
            .WithErrorCode(TaxonResult.Errors.SquareImageUrlInvalidFormat.Code);
    }

    [Fact(DisplayName = "SquareImageUrl: Should pass when valid")]
    public void ApplySquareImageUrlRules_WhenValid_ShouldNotHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { SquareImageUrl = "http://example.com/sq.png" });

        result.ShouldNotHaveValidationErrorFor(x => x.SquareImageUrl);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Taxon")]
public class TaxonValidationTaxonomyIdTests
{
    private sealed class TestModel
    {
        public Guid TaxonomyId { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.TaxonomyId).ApplyTaxonomyIdRules();
        }
    }

    [Fact(DisplayName = "TaxonomyId: Should fail when TaxonomyId is empty")]
    public void ApplyTaxonomyIdRules_WhenEmpty_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { TaxonomyId = Guid.Empty });

        result.ShouldHaveValidationErrorFor(x => x.TaxonomyId)
            .WithErrorCode(TaxonResult.Errors.InvalidTaxonomyId.Code);
    }

    [Fact(DisplayName = "TaxonomyId: Should pass when TaxonomyId is valid")]
    public void ApplyTaxonomyIdRules_WhenValid_ShouldNotHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { TaxonomyId = Guid.NewGuid() });

        result.ShouldNotHaveValidationErrorFor(x => x.TaxonomyId);
    }
}
