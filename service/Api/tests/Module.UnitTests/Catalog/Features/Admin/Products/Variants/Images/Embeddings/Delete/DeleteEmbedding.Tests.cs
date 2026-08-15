using Microsoft.EntityFrameworkCore;

using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Domain.Variants.Images.Embeddings;
using Module.Catalog.Features.Admin.Variants.Images.Embeddings.Delete;

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
        var image = VariantImageMethod.Create(
            "image/jpeg", "test.jpg", 1024, "https://cdn.test.com/test.jpg",
            "u/test.jpg", position: 0, variantId: Guid.NewGuid()).Value;
        _dbContext.Set<VariantImage>().Add(image);

        var embedding = ImageEmbeddingMethod.CreatePending(image.Id, "fashion-clip", "v1");
        _dbContext.Set<ImageEmbedding>().Add(embedding);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var command = new DeleteEmbedding.Command(image.Id);
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var deleted = await _dbContext.Set<ImageEmbedding>()
            .FirstOrDefaultAsync(e => e.VariantImageId == image.Id);
        deleted.Should().BeNull();
    }

    [Fact(DisplayName = "Handle: Returns 404 when no embedding exists")]
    public async Task Handle_ShouldReturnNotFound()
    {
        var command = new DeleteEmbedding.Command(Guid.NewGuid());
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors.First().Code.Should().Be("ImageEmbedding.VariantImageNotFound");
    }
}
