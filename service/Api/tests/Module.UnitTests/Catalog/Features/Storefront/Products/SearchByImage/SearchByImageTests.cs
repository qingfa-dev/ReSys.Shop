using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Domain.Products.Variants.Images.Embeddings;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Clients;
using Module.Catalog.Features.Storefront.Products.Shared.Services;

using Pgvector;

using SearchByImageFeature = Module.Catalog.Features.Storefront.Products.SearchByImage.SearchByImage;

namespace Module.UnitTests.Catalog.Features.Storefront.Products.SearchByImage;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "SearchByImage")]
public class SearchByImageTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IInferenceClient> _inferenceClientMock;
    private readonly SearchByImageFeature.PagedQueryHandler _handler;

    public SearchByImageTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Variant).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _inferenceClientMock = new Mock<IInferenceClient>();
        var vectorSearchService = new VectorSearchService(_dbContext);
        _handler = new SearchByImageFeature.PagedQueryHandler(_dbContext, _inferenceClientMock.Object, vectorSearchService);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return empty response when image is null")]
    public async Task Handle_ShouldReturnEmpty_WhenImageIsNull()
    {
        var request = new SearchByImageFeature.Request { Image = null! };
        var command = new SearchByImageFeature.Command(request);

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should return empty response when image is zero bytes")]
    public async Task Handle_ShouldReturnEmpty_WhenImageHasZeroBytes()
    {
        var formFile = CreateFormFile([], "test.jpg", "image/jpeg");
        var request = new SearchByImageFeature.Request { Image = formFile };
        var command = new SearchByImageFeature.Command(request);

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should return validation error when file exceeds 10 MB")]
    public async Task Handle_ShouldReturnValidationError_WhenFileTooLarge()
    {
        var bytes = new byte[10_485_761]; // 10 MB + 1 byte
        var formFile = CreateFormFile(bytes, "large.jpg", "image/jpeg");
        var request = new SearchByImageFeature.Request { Image = formFile };
        var command = new SearchByImageFeature.Command(request);

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.Code == "SearchByImage.FileTooLarge");
    }

    [Fact(DisplayName = "Handler: Should return validation error when content type is not image")]
    public async Task Handle_ShouldReturnValidationError_WhenNotImage()
    {
        var formFile = CreateFormFile([0x01, 0x02, 0x03], "doc.pdf", "application/pdf");
        var request = new SearchByImageFeature.Request { Image = formFile };
        var command = new SearchByImageFeature.Command(request);

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.Code == "SearchByImage.InvalidContentType");
    }

    [Fact(DisplayName = "Handler: Should return items when inference succeeds and gallery has matches")]
    public async Task Handle_ShouldReturnResults_WhenInferenceSucceeds()
    {
        // Arrange: Seed a variant with an embedding using the same vector the inference will return
        var product = new Product { Name = "Test Product", Slug = "test-product" };
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var variant = new Variant { ProductId = product.Id, Sku = "TEST-001", Price = 29.99m };
        _dbContext.Set<Variant>().Add(variant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var variantImage = new VariantImage
        {
            VariantId = variant.Id,
            Type = VariantImageType.Search,
            Url = "http://test.img/test.jpg",
            ContentType = "image/jpeg",
            FileName = "test.jpg",
            FileSize = 1024,
            Position = 0
        };
        _dbContext.Set<VariantImage>().Add(variantImage);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var vectorData = Enumerable.Repeat(0.1f, 512).ToArray();
        var embedding = new ImageEmbedding
        {
            Id = Guid.NewGuid(),
            VariantImageId = variantImage.Id,
            ModelName = "openclip-vit-b-32",
            ModelVersion = "1.0",
            Vector = new Vector(vectorData),
            Dimensions = 512
        };
        _dbContext.Set<ImageEmbedding>().Add(embedding);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var formFile = CreateFormFile([0xFF, 0xD8, 0xFF, 0xE0], "photo.jpg", "image/jpeg");
        var request = new SearchByImageFeature.Request { Image = formFile };
        var command = new SearchByImageFeature.Command(request);

        var inferenceResponse = new EmbeddingResponse
        {
            Vector = vectorData.ToList(),
            ModelVersion = "1.0",
            Dimension = 512
        };
        _inferenceClientMock
            .Setup(x => x.CreateEmbeddingFromBytesAsync(
                It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EmbeddingResponse>.Ok(inferenceResponse));

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
        result.Items.Should().ContainSingle(i => i.VariantId == variant.Id);
        _inferenceClientMock.Verify(
            x => x.CreateEmbeddingFromBytesAsync(
                It.IsAny<byte[]>(), "image/jpeg", "openclip-vit-b-32", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static IFormFile CreateFormFile(byte[] content, string fileName, string contentType)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "image", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
