using Microsoft.EntityFrameworkCore;

using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Variants;
using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Domain.Variants.Images.Embeddings;
using Module.Catalog.Features.Storefront.Products.Get.Similar;
using Module.Catalog.Features.Storefront.Products.Shared.Services;

using Pgvector;

namespace Module.UnitTests.Catalog.Features.Storefront.Products.Get.Similar;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "GetSimilarProducts")]
public class GetSimilarProductsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetSimilarProducts.PagedQueryHandler _handler;

    public GetSimilarProductsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Variant).Assembly];
        _dbContext = new ApplicationDbContext(options);

        var vectorSearchService = new VectorSearchService(_dbContext);
        _handler = new GetSimilarProducts.PagedQueryHandler(_dbContext, vectorSearchService);
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

        // Assert: No product found for this ID
        result.IsSuccess.Should().BeFalse();
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
            new GetSimilarProducts.Query(product.Id),
            TestContext.Current.CancellationToken);

        // Assert: No embedding -> returns empty
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should return similar products when embeddings exist")]
    public async Task Handle_ShouldReturnSimilarProducts_WhenEmbeddingsExist()
    {
        // Arrange: Seed source variant with an embedding
        var sourceProduct = new Product { Name = "Source Product", Slug = "source-product" };
        _dbContext.Set<Product>().Add(sourceProduct);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var sourceVariant = new Variant { ProductId = sourceProduct.Id, Sku = "SRC-001" };
        _dbContext.Set<Variant>().Add(sourceVariant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var sourceImage = new VariantImage
        {
            VariantId = sourceVariant.Id,
            Type = VariantImageType.Search,
            Url = "http://test.img/src.jpg",
            ContentType = "image/jpeg",
            FileName = "src.jpg",
            FileSize = 1024,
            Position = 0
        };
        _dbContext.Set<VariantImage>().Add(sourceImage);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var sourceVectorData = Enumerable.Repeat(0.1f, 384).ToArray();
        var sourceEmbedding = new ImageEmbedding
        {
            Id = Guid.NewGuid(),
            VariantImageId = sourceImage.Id,
            ModelName = VariantImageConstant.Defaults.DefaultSimilarityModel,
            ModelVersion = "1.0",
            Vector = new Vector(sourceVectorData),
            Dimensions = 384
        };
        _dbContext.Set<ImageEmbedding>().Add(sourceEmbedding);

        // Arrange: Seed a similar variant with the same vector (zero cosine distance)
        var similarProduct = new Product { Name = "Similar Product", Slug = "similar-product" };
        _dbContext.Set<Product>().Add(similarProduct);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var similarVariant = new Variant { ProductId = similarProduct.Id, Sku = "SIM-001", Price = 19.99m };
        _dbContext.Set<Variant>().Add(similarVariant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var similarImage = new VariantImage
        {
            VariantId = similarVariant.Id,
            Type = VariantImageType.Search,
            Url = "http://test.img/sim.jpg",
            ContentType = "image/jpeg",
            FileName = "sim.jpg",
            FileSize = 1024,
            Position = 0
        };
        _dbContext.Set<VariantImage>().Add(similarImage);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var similarEmbedding = new ImageEmbedding
        {
            Id = Guid.NewGuid(),
            VariantImageId = similarImage.Id,
            ModelName = VariantImageConstant.Defaults.DefaultSimilarityModel,
            ModelVersion = "1.0",
            Vector = new Vector(sourceVectorData),
            Dimensions = 384
        };
        _dbContext.Set<ImageEmbedding>().Add(similarEmbedding);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new GetSimilarProducts.Query(sourceProduct.Id),
            TestContext.Current.CancellationToken);

        // Assert: Similar product should be returned, source product should be excluded
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
        result.Items.Should().ContainSingle(i => i.Id == similarProduct.Id);
        result.Items.Should().NotContain(i => i.Id == sourceProduct.Id);
        result.Items.Single(i => i.Id == similarProduct.Id).SimilarityScore.Should().BeApproximately(1.0, 1e-6);
    }
}
