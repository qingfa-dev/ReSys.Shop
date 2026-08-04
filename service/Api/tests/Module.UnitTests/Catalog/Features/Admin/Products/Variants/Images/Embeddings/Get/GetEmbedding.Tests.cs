using Microsoft.EntityFrameworkCore;
using Module.Catalog.Domain.Products.Variants.Images.Embeddings;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Get;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "GetEmbedding")]
public class GetEmbeddingTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetEmbedding.QueryHandler _handler;

    public GetEmbeddingTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ImageEmbedding).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetEmbedding.QueryHandler(_dbContext);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Handle: Returns EmbeddingDetailResponse with status fields")]
    public async Task Handle_ShouldReturnEmbeddingDetail()
    {
        var variantImageId = Guid.NewGuid();
        var embedding = ImageEmbeddingMethod.Create(variantImageId, "fashion-clip", "v1", [0.1f, 0.2f]);
        embedding.Status = EmbeddingStatus.Completed;
        embedding.HangfireJobId = "job-123";
        embedding.CompletedAtUtc = DateTimeOffset.UtcNow;
        _dbContext.Set<ImageEmbedding>().Add(embedding);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetEmbedding.Query(variantImageId), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Completed");
        result.Value.HangfireJobId.Should().Be("job-123");
        result.Value.CompletedAtUtc.Should().NotBeNull();
    }

    [Fact(DisplayName = "Handle: Returns 404 when no embedding exists")]
    public async Task Handle_ShouldReturnNotFound()
    {
        var result = await _handler.Handle(
            new GetEmbedding.Query(Guid.NewGuid()), TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
        result.Errors.First().Code.Should().Be("ImageEmbedding.VariantImageNotFound");
    }
}
