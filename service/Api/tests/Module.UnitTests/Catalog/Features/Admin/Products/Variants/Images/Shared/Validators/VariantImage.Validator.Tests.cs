using Microsoft.AspNetCore.Http;

using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Features.Admin.Shared.Models;
using Module.Catalog.Features.Admin.Shared.Validators;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Images.Shared.Validators;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "VariantImageValidator")]
public class VariantImageParametersValidatorTests
{
    private readonly VariantImageValidator.VariantImageParametersValidator _sut = new();

    [Fact(DisplayName = "Parameters: Should fail when alt exceeds max length")]
    public void Validate_WhenAltTooLong_ShouldHaveError()
    {
        var model = new TestParameters { Alt = new string('a', VariantImageConstant.Constraints.AltMaxLength + 1) };
        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Alt)
            .WithErrorCode(VariantImageResult.Failure.AltTooLong.Code);
    }

    [Theory(DisplayName = "Parameters: Should pass with valid alt")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("valid alt text")]
    public void Validate_WhenAltValid_ShouldNotHaveError(string? alt)
    {
        var model = new TestParameters { Alt = alt };
        var result = _sut.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Alt);
    }

    [Fact(DisplayName = "Parameters: Should fail when position is negative")]
    public void Validate_WhenPositionNegative_ShouldHaveError()
    {
        var model = new TestParameters { Position = -1 };
        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Position)
            .WithErrorCode(VariantImageResult.Failure.InvalidPosition.Code);
    }

    [Fact(DisplayName = "Parameters: Should pass with valid position")]
    public void Validate_WhenPositionValid_ShouldNotHaveError()
    {
        var model = new TestParameters { Position = 0 };
        var result = _sut.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Position);
    }

    [Fact(DisplayName = "Parameters: Should fail when type is invalid")]
    public void Validate_WhenTypeInvalid_ShouldHaveError()
    {
        var model = new TestParameters { Type = (VariantImageType)999 };
        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorCode(VariantImageResult.Failure.InvalidType.Code);
    }

    [Theory(DisplayName = "Parameters: Should pass with valid type")]
    [InlineData(VariantImageType.Default)]
    [InlineData(VariantImageType.Thumbnail)]
    [InlineData(VariantImageType.Gallery)]
    public void Validate_WhenTypeValid_ShouldNotHaveError(VariantImageType type)
    {
        var model = new TestParameters { Type = type };
        var result = _sut.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    private sealed record TestParameters : VariantImageParameters;
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "VariantImageValidator")]
public class UploadImageRequestValidatorTests
{
    private readonly VariantImageValidator.UploadImageRequestValidator _sut = new();

    [Fact(DisplayName = "Upload: Should fail when file is null")]
    public void Validate_WhenFileNull_ShouldHaveError()
    {
        var model = new UploadImageRequest { File = null! };
        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.File)
            .WithErrorCode(VariantImageResult.Failure.FileRequired.Code);
    }

    [Fact(DisplayName = "Upload: Should fail when file is empty")]
    public void Validate_WhenFileEmpty_ShouldHaveError()
    {
        var file = new FormFile(Stream.Null, 0, 0, "file", "empty.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
        var model = new UploadImageRequest { File = file };
        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.File.Length)
            .WithErrorCode(VariantImageResult.Failure.FileEmpty.Code);
    }

    [Fact(DisplayName = "Upload: Should fail when content type is not allowed")]
    public void Validate_WhenContentTypeInvalid_ShouldHaveError()
    {
        var file = new FormFile(Stream.Null, 0, 1024, "file", "test.bmp")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/bmp"
        };
        var model = new UploadImageRequest { File = file };
        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.File.ContentType)
            .WithErrorCode(VariantImageResult.Failure.InvalidContentType.Code);
    }

    [Fact(DisplayName = "Upload: Should fail when file exceeds max size")]
    public void Validate_WhenFileTooLarge_ShouldHaveError()
    {
        var oversized = VariantImageConstant.Constraints.Upload.MaxFileSizeBytes + 1;
        var file = new FormFile(Stream.Null, 0, oversized, "file", "large.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
        var model = new UploadImageRequest { File = file };
        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.File.Length)
            .WithErrorCode(VariantImageResult.Failure.FileTooLarge.Code);
    }

    [Theory(DisplayName = "Upload: Should fail when extension is disallowed")]
    [InlineData(".exe")]
    [InlineData(".php")]
    [InlineData(".html")]
    [InlineData(".bmp")]
    public void Validate_WhenExtensionDisallowed_ShouldHaveError(string extension)
    {
        var file = new FormFile(Stream.Null, 0, 1024, "file", $"evil{extension}")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
        var model = new UploadImageRequest { File = file };
        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.File.FileName);
    }

    [Theory(DisplayName = "Upload: Should pass when extension is allowed")]
    [InlineData(".jpg")]
    [InlineData(".jpeg")]
    [InlineData(".png")]
    [InlineData(".gif")]
    [InlineData(".webp")]
    public void Validate_WhenExtensionAllowed_ShouldNotHaveError(string extension)
    {
        var file = new FormFile(new MemoryStream(new byte[1024]), 0, 1024, "file", $"image{extension}")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
        var model = new UploadImageRequest { File = file, Position = 1, Type = VariantImageType.Gallery };
        var result = _sut.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Upload: Should pass with valid file")]
    public void Validate_WhenValid_ShouldNotHaveError()
    {
        var file = new FormFile(new MemoryStream(new byte[1024]), 0, 1024, "file", "photo.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
        var model = new UploadImageRequest { File = file, Position = 1, Type = VariantImageType.Gallery };
        var result = _sut.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "VariantImageValidator")]
public class UpdateImageRequestValidatorTests
{
    private readonly VariantImageValidator.UpdateImageRequestValidator _sut = new();

    [Fact(DisplayName = "Update: Should pass with valid update request")]
    public void Validate_WhenValid_ShouldNotHaveError()
    {
        var model = new UpdateImageRequest { Alt = "Updated alt", Position = 2, Type = VariantImageType.Thumbnail };
        var result = _sut.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Update: Should fail when position is negative")]
    public void Validate_WhenPositionNegative_ShouldHaveError()
    {
        var model = new UpdateImageRequest { Position = -1 };
        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor("Position")
            .WithErrorCode(VariantImageResult.Failure.InvalidPosition.Code);
    }

    [Fact(DisplayName = "Update: Should fail when type is invalid")]
    public void Validate_WhenTypeInvalid_ShouldHaveError()
    {
        var model = new UpdateImageRequest { Type = (VariantImageType)999 };
        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor("Type")
            .WithErrorCode(VariantImageResult.Failure.InvalidType.Code);
    }
}
