using Microsoft.Extensions.Logging.Abstractions;

using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Domain.Products.Variants.Images.Embeddings;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Clients;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Services;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "EmbeddingOrchestrator")]
public class EmbeddingOrchestratorRunAsyncTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IInferenceClient> _inferenceClientMock;
    private readonly EmbeddingOrchestrator _orchestrator;

    public EmbeddingOrchestratorRunAsyncTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ImageEmbedding).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _inferenceClientMock = new Mock<IInferenceClient>();
        _orchestrator = new EmbeddingOrchestrator(
            _inferenceClientMock.Object, _dbContext,
            Microsoft.Extensions.Options.Options.Create(new EmbeddingOrchestratorOptions { DefaultModel = "fashion-clip" }),
            NullLogger<EmbeddingOrchestrator>.Instance);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "RunAsync: Happy path Pending -> Completed")]
    public async Task RunAsync_ShouldComplete_Successfully()
    {
        var image = VariantImageMethod.Create(
            "image/jpeg", "test.jpg", 1000, "https://cdn.test.com/test.jpg",
            "u/test.jpg", position: 0, variantId: Guid.NewGuid()).Value;
        _dbContext.Set<VariantImage>().Add(image);
        var embedding = ImageEmbeddingMethod.CreatePending(image.Id, "fashion-clip", "v1");
        _dbContext.Set<ImageEmbedding>().Add(embedding);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _inferenceClientMock.Setup(c => c.CreateEmbeddingAsync(
            It.Is<EmbeddingRequest>(r => r.ImageUrl == image.Url), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EmbeddingResponse>.Ok(new EmbeddingResponse
                { Vector = [0.1f, 0.2f], Dimension = 2, ModelVersion = "v1" }));

        var result = await _orchestrator.RunAsync(embedding.Id, TestContext.Current.CancellationToken);
        result.IsSuccess.Should().BeTrue();

        var updated = await _dbContext.Set<ImageEmbedding>().FindAsync(embedding.Id);
        updated!.Status.Should().Be(EmbeddingStatus.Completed);
        updated.Dimensions.Should().Be(2);
        updated.CompletedAtUtc.Should().NotBeNull();
        updated.Vector.Should().NotBeNull();
        updated.Vector.ToArray().Should().Equal(0.1f, 0.2f);
    }

    [Fact(DisplayName = "RunAsync: Should mark Failed on inference failure")]
    public async Task RunAsync_ShouldFail_WhenInferenceFails()
    {
        var image = VariantImageMethod.Create(
            "image/jpeg", "test.jpg", 1000, "https://cdn.test.com/test.jpg",
            "u/test.jpg", position: 0, variantId: Guid.NewGuid()).Value;
        _dbContext.Set<VariantImage>().Add(image);
        var embedding = ImageEmbeddingMethod.CreatePending(image.Id, "fashion-clip", "v1");
        _dbContext.Set<ImageEmbedding>().Add(embedding);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _inferenceClientMock.Setup(c => c.CreateEmbeddingAsync(
            It.IsAny<EmbeddingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImageEmbeddingResult.Errors.RequestTimeout);

        var result = await _orchestrator.RunAsync(embedding.Id, TestContext.Current.CancellationToken);
        result.IsSuccess.Should().BeTrue();

        var updated = await _dbContext.Set<ImageEmbedding>().FindAsync(embedding.Id);
        updated!.Status.Should().Be(EmbeddingStatus.Failed);
        updated.Error.Should().NotBeNullOrEmpty();
    }

    [Fact(DisplayName = "RunAsync: Should mark Failed when image deleted")]
    public async Task RunAsync_ShouldFail_WhenImageNotFound()
    {
        var embedding = ImageEmbeddingMethod.CreatePending(Guid.NewGuid(), "fashion-clip", "v1");
        _dbContext.Set<ImageEmbedding>().Add(embedding);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _orchestrator.RunAsync(embedding.Id, TestContext.Current.CancellationToken);
        result.IsSuccess.Should().BeTrue();

        var updated = await _dbContext.Set<ImageEmbedding>().FindAsync(embedding.Id);
        updated!.Status.Should().Be(EmbeddingStatus.Failed);
        updated.Error.Should().Contain("Image was deleted");
    }

    [Fact(DisplayName = "RunAsync: Should return failure when embedding not found")]
    public async Task RunAsync_ShouldReturnFailure_WhenEmbeddingNotFound()
    {
        var result = await _orchestrator.RunAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
    }
}
