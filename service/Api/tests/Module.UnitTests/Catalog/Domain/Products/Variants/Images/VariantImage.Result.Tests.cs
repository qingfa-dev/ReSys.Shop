using Module.Catalog.Domain.Variants.Images;

namespace Module.UnitTests.Catalog.Domain.Products.Variants.Images;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "VariantImage")]
public class VariantImageResultUnsupportedFileTypeTests
{
    [Fact(DisplayName = "UnsupportedFileType: returns validation error with extension in message")]
    public void UnsupportedFileType_ReturnsValidationError()
    {
        var error = VariantImageResult.Failure.UnsupportedFileType(".exe");
        error.Code.Should().Be("VariantImage.UnsupportedFileType");
        error.Type.Should().Be(422);
        error.Message.Should().Contain(".exe");
    }
}
