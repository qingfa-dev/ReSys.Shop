using Hangfire;
using Hangfire.Common;
using Hangfire.States;

using Microsoft.Extensions.Logging.Abstractions;

using Module.Catalog.Domain.Variants.Images.Embeddings;
using Module.Catalog.Features.Admin.Variants.Images.Embeddings.Regenerate;
using Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Services;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Regenerate;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "RegenerateEmbedding")]
public class RegenerateEmbeddingTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IBackgroundJobClient> _bgJobMock;
    private readonly Mock<IEmbeddingOrchestrator> _orchestratorMock;
    private readonly RegenerateEmbedding.CommandHandler _handler;

    public RegenerateEmbeddingTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ImageEmbedding).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _bgJobMock = new Mock<IBackgroundJobClient>();
        _orchestratorMock = new Mock<IEmbeddingOrchestrator>();
        _handler = new RegenerateEmbedding.CommandHandler(
            _orchestratorMock.Object, _dbContext, _bgJobMock.Object,
            NullLogger<RegenerateEmbedding.CommandHandler>.Instance);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Handle: Transitions existing Completed to Pending and enqueues")]
    public async Task Handle_ShouldResetExistingAndEnqueue()
    {
        var variantImageId = Guid.NewGuid();
        var embedding = ImageEmbeddingMethod.Create(variantImageId, "fashion-clip", "v1", [0.1f]);
        _dbContext.Set<ImageEmbedding>().Add(embedding);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var jobId = "regenerate-job";
        _bgJobMock.Setup(b => b.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>())).Returns(jobId);

        var command = new RegenerateEmbedding.Command(new RegenerateEmbedding.Request
            { VariantImageId = variantImageId, ModelName = "fashion-clip", ModelVersion = "v2" });

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(EmbeddingStatus.Pending);
        result.Value.HangfireJobId.Should().Be(jobId);

        var saved = await _dbContext.Set<ImageEmbedding>()
            .FirstAsync(e => e.VariantImageId == variantImageId);
        saved.Status.Should().Be(EmbeddingStatus.Pending);
        saved.Error.Should().BeNull();
        _bgJobMock.Verify(b => b.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()), Times.Once);
    }

    [Fact(DisplayName = "Handle: Creates new Pending row if embedding was deleted")]
    public async Task Handle_ShouldCreateNewRow_WhenNoneExists()
    {
        var variantImageId = Guid.NewGuid();
        var jobId = "regenerate-job-2";
        _bgJobMock.Setup(b => b.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>())).Returns(jobId);

        var command = new RegenerateEmbedding.Command(new RegenerateEmbedding.Request
            { VariantImageId = variantImageId, ModelName = "fashion-clip" });

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(EmbeddingStatus.Pending);

        var saved = await _dbContext.Set<ImageEmbedding>()
            .FirstOrDefaultAsync(e => e.VariantImageId == variantImageId);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(EmbeddingStatus.Pending);
    }
}
