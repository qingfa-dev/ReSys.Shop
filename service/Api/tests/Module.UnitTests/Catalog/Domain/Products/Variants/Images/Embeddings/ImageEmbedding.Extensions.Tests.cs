using Module.Catalog.Domain.Products.Variants.Images.Embeddings;

namespace Module.UnitTests.Catalog.Domain.Products.VariantImageMethod.Embeddings;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "ImageEmbedding")]
public class ImageEmbeddingExtensionsTests
{
    [Fact(DisplayName = "Create: Should return ImageEmbedding with correct properties")]
    public void Create_WithValidParameters_ShouldReturnImageEmbedding()
    {
        var variantImageId = Guid.NewGuid();
        var modelName = "resnet50";
        var modelVersion = "v1";
        var vectorData = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };

        var result = ImageEmbeddingExtensions.Create(variantImageId, modelName, modelVersion, vectorData);

        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.VariantImageId.Should().Be(variantImageId);
        result.ModelName.Should().Be(modelName);
        result.ModelVersion.Should().Be(modelVersion);
        result.Dimensions.Should().Be(vectorData.Length);
    }
}
