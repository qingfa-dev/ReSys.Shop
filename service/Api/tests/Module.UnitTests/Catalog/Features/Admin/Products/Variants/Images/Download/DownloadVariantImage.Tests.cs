using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Features.Admin.Variants.Images.Download;
using Module.Catalog.Features.Admin.Variants.Images.Shared.Models;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Images.Download;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "VariantImageDownload")]
public class DownloadVariantImageTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly DownloadVariantImage.QueryHandler _handler;

    public DownloadVariantImageTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(VariantImage).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _storageServiceMock = new Mock<IStorageService>();

        _handler = new DownloadVariantImage.QueryHandler(_dbContext, _storageServiceMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return file stream when image found")]
    public async Task Handle_ShouldReturnStream_WhenFound()
    {
        var image = Module.Catalog.Domain.Variants.Images.VariantImageMethod.Create("image/png", "screenshot.png", 4096,
            url: "https://cdn.test.com/screenshot.png", storagePath: "uploads/screenshot.png",
            variantId: Guid.NewGuid()).Value;
        _dbContext.Set<VariantImage>().Add(image);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var stream = new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
        _storageServiceMock
            .Setup(x => x.DownloadAsync(image.StoragePath, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DownloadResult>.Ok(new DownloadResult { Content = stream, Info = new StoredObjectInfo { Key = "key", Provider = "provider", SizeBytes = stream.Length, LastModifiedUtc = DateTimeOffset.UtcNow, ContentType = "image/png" } }));

        var result = await _handler.Handle(
            new DownloadVariantImage.Query(image.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeAssignableTo<VariantImageDownloadResponse>();
        result.Value.ContentType.Should().Be("image/png");
        result.Value.FileName.Should().Be("screenshot.png");
        result.Value.Url.Should().Be("https://cdn.test.com/screenshot.png");
        result.Value.Stream.Should().BeSameAs(stream);
    }

    [Fact(DisplayName = "Handler: Should return failure when image not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        var result = await _handler.Handle(
            new DownloadVariantImage.Query(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(VariantImageResult.Failure.ById(Guid.Empty).Code);
    }

    [Fact(DisplayName = "Handler: Should propagate storage download failure")]
    public async Task Handle_ShouldReturnFailure_WhenStorageFails()
    {
        var image = Module.Catalog.Domain.Variants.Images.VariantImageMethod.Create("image/jpeg", "photo.jpg", 1024,
            url: "https://cdn.test.com/photo.jpg", storagePath: "uploads/photo.jpg",
            variantId: Guid.NewGuid()).Value;
        _dbContext.Set<VariantImage>().Add(image);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _storageServiceMock
            .Setup(x => x.DownloadAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DownloadResult>.Unexpected(errors: [Error.Unexpected("Storage.DownloadError", "Download failed")]));

        var result = await _handler.Handle(
            new DownloadVariantImage.Query(image.Id),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Storage.DownloadError");
    }
}
