
using Module.Catalog.Domain.Products.Variants.Images;

namespace Module.UnitTests.Catalog.Domain.Products.Variants.Images;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "VariantImage")]
public class VariantImageTests
{
    [Fact(DisplayName = "Create: Should return VariantImage with correct properties")]
    public void Create_WithValidParameters_ShouldReturnVariantImage()
    {
        // Arrange
        var variantId = Guid.NewGuid();
        var contentType = "image/jpeg";
        var fileName = "image.jpg";
        var fileSize = 1024;

        // Act
        var result = Module.Catalog.Domain.Products.Variants.Images.VariantImageMethod.Create(contentType, fileName, fileSize, url: "https://example.com/img.jpg", storagePath: "uploads/img.jpg", variantId: variantId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.VariantId.Should().Be(variantId);
        result.Value.ContentType.Should().Be(contentType);
        result.Value.FileName.Should().Be(fileName);
        result.Value.FileSize.Should().Be(fileSize);
    }

    [Fact(DisplayName = "UpdateDetails: Should update image metadata")]
    public void UpdateDetails_WithValidParameters_ShouldUpdateMetadata()
    {
        var imageResult = Module.Catalog.Domain.Products.Variants.Images.VariantImageMethod.Create("image/jpeg", "img.jpg", 1024, url: "https://example.com/img.jpg", storagePath: "uploads/img.jpg");
        imageResult.IsSuccess.Should().BeTrue();
        var image = imageResult.Value;
        var position = 1;
        var alt = "Alt Text";
        var type = VariantImageType.Thumbnail;

        var result = image.UpdateDetails(position, alt, type);

        result.IsSuccess.Should().BeTrue();
        image.Position.Should().Be(position);
        image.Alt.Should().Be(alt);
        image.Type.Should().Be(type);
    }

    [Fact(DisplayName = "UpdateDetails: Partial update should preserve other values")]
    public void UpdateDetails_WithOnlyPosition_ShouldPreserveOthers()
    {
        var imageResult = Module.Catalog.Domain.Products.Variants.Images.VariantImageMethod.Create("image/jpeg", "img.jpg", 1024, url: "https://example.com/img.jpg", storagePath: "uploads/img.jpg", alt: "Old Alt");
        imageResult.IsSuccess.Should().BeTrue();
        var image = imageResult.Value;

        var result = image.UpdateDetails(position: 5);

        result.IsSuccess.Should().BeTrue();
        image.Position.Should().Be(5);
        image.Alt.Should().Be("Old Alt");
    }
}
