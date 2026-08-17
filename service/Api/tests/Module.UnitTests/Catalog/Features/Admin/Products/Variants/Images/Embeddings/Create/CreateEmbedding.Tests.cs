using Hangfire;
using Hangfire.Common;
using Hangfire.States;

using Microsoft.Extensions.Logging.Abstractions;

using Module.Catalog.Domain.Variants.Images.Embeddings;
using Module.Catalog.Features.Admin.Variants.Images.Embeddings.Create;
using Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Services;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "CreateEmbedding")]
public class CreateEmbeddingTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IBackgroundJobClient> _bgJobMock;
    private readonly Mock<IEmbeddingOrchestrator> _orchestratorMock;
    private readonly CreateEmbedding.CommandHandler _handler;

    public CreateEmbeddingTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ImageEmbedding).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _bgJobMock = new Mock<IBackgroundJobClient>();
        _orchestratorMock = new Mock<IEmbeddingOrchestrator>();
        _handler = new CreateEmbedding.CommandHandler(
            _orchestratorMock.Object, _dbContext, _bgJobMock.Object,
            NullLogger<CreateEmbedding.CommandHandler>.Instance);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Handle: Creates Pending row and enqueues Hangfire job")]
    public async Task Handle_ShouldCreatePendingAndEnqueue()
    {
        var variantImageId = Guid.NewGuid();
        var jobId = "bg-job-1";
        _bgJobMock.Setup(b => b.Create(
            It.IsAny<Job>(), It.IsAny<EnqueuedState>())).Returns(jobId);

        var command = new CreateEmbedding.Command(new CreateEmbedding.Request
            { VariantImageId = variantImageId, ModelName = "fashion-clip" });

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(EmbeddingStatus.Pending);
        result.Value.HangfireJobId.Should().Be(jobId);

        var saved = await _dbContext.Set<ImageEmbedding>()
            .FirstOrDefaultAsync(e => e.VariantImageId == variantImageId);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(EmbeddingStatus.Pending);
        _bgJobMock.Verify(b => b.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()), Times.Once);
    }

    [Fact(DisplayName = "Handle: Returns Conflict when Pending row exists")]
    public async Task Handle_ShouldReturnConflict_WhenPendingExists()
    {
        var variantImageId = Guid.NewGuid();
        _dbContext.Set<ImageEmbedding>().Add(
            ImageEmbeddingMethod.CreatePending(variantImageId, "fashion-clip", "v1"));
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var command = new CreateEmbedding.Command(new CreateEmbedding.Request
            { VariantImageId = variantImageId, ModelName = "fashion-clip" });

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
        result.Errors.First().Code.Should().Be("ImageEmbedding.Conflict");
    }
}
