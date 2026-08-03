using Module.Catalog.Domain.Products.Variants.Images.Embeddings;

namespace Module.UnitTests.Catalog.Domain.Products.Variants.Images.Embeddings;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "ImageEmbedding")]
public class ImageEmbeddingMethodTests
{
    [Fact(DisplayName = "Create: Should return ImageEmbedding with correct properties")]
    public void Create_WithValidParameters_ShouldReturnImageEmbedding()
    {
        var variantImageId = Guid.NewGuid();
        var modelName = "resnet50";
        var modelVersion = "v1";
        var vectorData = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };

        var result = ImageEmbeddingMethod.Create(variantImageId, modelName, modelVersion, vectorData);

        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.VariantImageId.Should().Be(variantImageId);
        result.ModelName.Should().Be(modelName);
        result.ModelVersion.Should().Be(modelVersion);
        result.Dimensions.Should().Be(vectorData.Length);
    }

    [Fact(DisplayName = "Create: Should set Status to Completed by default")]
    public void Create_ShouldSetStatusToCompleted()
    {
        var variantImageId = Guid.NewGuid();
        var vectorData = new float[] { 0.1f, 0.2f, 0.3f };

        var result = ImageEmbeddingMethod.Create(variantImageId, "resnet50", "v1", vectorData);

        result.Status.Should().Be(EmbeddingStatus.Completed);
        result.Error.Should().BeNull();
        result.HangfireJobId.Should().BeNull();
        result.CompletedAtUtc.Should().BeNull();
    }

    [Fact(DisplayName = "CreatePending: Should create embedding with Pending status")]
    public void CreatePending_ShouldCreatePendingEmbedding()
    {
        var variantImageId = Guid.NewGuid();
        var result = ImageEmbeddingMethod.CreatePending(variantImageId, "fashion-clip", "v2");
        result.Status.Should().Be(EmbeddingStatus.Pending);
        result.Dimensions.Should().Be(0);
        result.HangfireJobId.Should().BeNull();
        result.Error.Should().BeNull();
    }

    [Fact(DisplayName = "MarkProcessing: Should transition Pending to Processing")]
    public void MarkProcessing_ShouldTransitionPendingToProcessing()
    {
        var embedding = ImageEmbeddingMethod.CreatePending(Guid.NewGuid(), "m", "v1");
        var result = ImageEmbeddingMethod.MarkProcessing(embedding);
        result.IsSuccess.Should().BeTrue();
        embedding.Status.Should().Be(EmbeddingStatus.Processing);
    }

    [Fact(DisplayName = "MarkProcessing: Should fail when not Pending")]
    public void MarkProcessing_ShouldFail_WhenNotPending()
    {
        var embedding = ImageEmbeddingMethod.Create(Guid.NewGuid(), "m", "v1", [0.1f]);
        var result = ImageEmbeddingMethod.MarkProcessing(embedding);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "MarkCompleted: Should store vector and transition")]
    public void MarkCompleted_ShouldStoreVectorAndTransition()
    {
        var embedding = ImageEmbeddingMethod.CreatePending(Guid.NewGuid(), "m", "v1");
        ImageEmbeddingMethod.MarkProcessing(embedding);
        var vector = new float[] { 0.1f, 0.2f, 0.3f };

        var result = ImageEmbeddingMethod.MarkCompleted(embedding, vector, 3, "v2");

        result.IsSuccess.Should().BeTrue();
        embedding.Status.Should().Be(EmbeddingStatus.Completed);
        embedding.Dimensions.Should().Be(3);
        embedding.ModelVersion.Should().Be("v2");
        embedding.CompletedAtUtc.Should().NotBeNull();
    }

    [Fact(DisplayName = "MarkCompleted: Should fail when not Processing")]
    public void MarkCompleted_ShouldFail_WhenNotProcessing()
    {
        var embedding = ImageEmbeddingMethod.CreatePending(Guid.NewGuid(), "m", "v1");
        var result = ImageEmbeddingMethod.MarkCompleted(embedding, [0.1f], 1, "v1");
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "MarkFailed: Should set error and transition")]
    public void MarkFailed_ShouldSetErrorAndTransition()
    {
        var embedding = ImageEmbeddingMethod.CreatePending(Guid.NewGuid(), "m", "v1");
        ImageEmbeddingMethod.MarkProcessing(embedding);

        var result = ImageEmbeddingMethod.MarkFailed(embedding, "Inference timeout");

        result.IsSuccess.Should().BeTrue();
        embedding.Status.Should().Be(EmbeddingStatus.Failed);
        embedding.Error.Should().Be("Inference timeout");
        embedding.CompletedAtUtc.Should().NotBeNull();
    }

    [Fact(DisplayName = "MarkFailed: Should fail when not Processing")]
    public void MarkFailed_ShouldFail_WhenNotProcessing()
    {
        var embedding = ImageEmbeddingMethod.CreatePending(Guid.NewGuid(), "m", "v1");
        var result = ImageEmbeddingMethod.MarkFailed(embedding, "error");
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "MarkPending: Should reset Completed to Pending")]
    public void MarkPending_ShouldResetToPending()
    {
        var embedding = ImageEmbeddingMethod.Create(Guid.NewGuid(), "m", "v1", [0.1f]);
        embedding.HangfireJobId = "job-1";
        embedding.Error = "old error";

        var result = ImageEmbeddingMethod.MarkPending(embedding);

        result.IsSuccess.Should().BeTrue();
        embedding.Status.Should().Be(EmbeddingStatus.Pending);
        embedding.Error.Should().BeNull();
        embedding.HangfireJobId.Should().BeNull();
    }

    [Fact(DisplayName = "MarkPending: Should be no-op when already Pending")]
    public void MarkPending_ShouldBeNoOp_WhenAlreadyPending()
    {
        var embedding = ImageEmbeddingMethod.CreatePending(Guid.NewGuid(), "m", "v1");
        var result = ImageEmbeddingMethod.MarkPending(embedding);
        result.IsSuccess.Should().BeTrue();
        embedding.Status.Should().Be(EmbeddingStatus.Pending);
    }
}
