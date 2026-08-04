using Microsoft.EntityFrameworkCore;
using Module.Catalog.Domain.Products.Variants.Images.Embeddings;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Delete;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "DeleteEmbedding")]
public class DeleteEmbeddingTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DeleteEmbedding.CommandHandler _handler;

    public DeleteEmbeddingTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ImageEmbedding).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new DeleteEmbedding.CommandHandler(_dbContext);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Handle: Removes embedding and returns 200 with message")]
    public async Task Handle_ShouldDeleteAndReturn200()
    {
        var variantImageId = Guid.NewGuid();
        var embedding = ImageEmbeddingMethod.CreatePending(variantImageId, "fashion-clip", "v1");
        _dbContext.Set<ImageEmbedding>().Add(embedding);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var command = new DeleteEmbedding.Command(variantImageId);
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Message.Should().Contain("deleted");

        var deleted = await _dbContext.Set<ImageEmbedding>()
            .FirstOrDefaultAsync(e => e.VariantImageId == variantImageId);
        deleted.Should().BeNull();
    }

    [Fact(DisplayName = "Handle: Returns 404 when no embedding exists")]
    public async Task Handle_ShouldReturnNotFound()
    {
        var command = new DeleteEmbedding.Command(Guid.NewGuid());
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors.First().Code.Should().Be("ImageEmbedding.NotFound");
    }
}
