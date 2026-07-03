using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Features.Storefront.Products.Get.Similar;

namespace Module.UnitTests.Catalog.Features.Storefront.Products.Get.Similar;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "GetSimilarProducts")]
public class GetSimilarProductsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetSimilarProducts.QueryHandler _handler;

    public GetSimilarProductsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Variant).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetSimilarProducts.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return empty result when variant not found")]
    public async Task Handle_ShouldReturnEmpty_WhenVariantNotFound()
    {
        // Act
        var result = await _handler.Handle(
            new GetSimilarProducts.Query(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should return empty result when variant has no embedding")]
    public async Task Handle_ShouldReturnEmpty_WhenNoEmbedding()
    {
        // Arrange: Create variant without embedding (InMemory can't run pgvector SQL)
        var product = new Product { Name = "Test Product", Slug = "test-product" };
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var variant = new Variant { ProductId = product.Id, Sku = "TEST-001", Price = 29.99m };
        _dbContext.Set<Variant>().Add(variant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new GetSimilarProducts.Query(variant.Id),
            TestContext.Current.CancellationToken);

        // Assert: No embedding -> returns empty
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }
}
