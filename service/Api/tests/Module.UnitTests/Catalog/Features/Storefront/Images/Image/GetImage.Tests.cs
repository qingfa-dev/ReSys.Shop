using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Features.Storefront.Images.Get.Image;
using Shared.Operational.Storages.Models;

namespace Module.UnitTests.Catalog.Features.Storefront.Images.Image;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontGetImage")]
public class GetImageTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly GetImage.QueryHandler _handler;

    public GetImageTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _storageServiceMock = new Mock<IStorageService>();

        _handler = new GetImage.QueryHandler(_dbContext, _storageServiceMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return stream and content type when VariantImage exists")]
    public async Task Handle_ShouldReturnStream_WhenImageExists()
    {
        var image = new VariantImage
        {
            Id = Guid.NewGuid(),
            FileName = "test.jpg",
            ContentType = "image/jpeg",
            StoragePath = "images/test.jpg",
            Url = string.Empty
        };
        _dbContext.Set<VariantImage>().Add(image);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        _storageServiceMock
            .Setup(s => s.DownloadAsync(image.StoragePath, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DownloadResult>.Ok(new DownloadResult { Content = stream }));

        var result = await _handler.Handle(
            new GetImage.Query(image.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Stream.Should().NotBeNull();
        result.Value.ContentType.Should().Be("image/jpeg");
    }

    [Fact(DisplayName = "Handler: Should return failure when VariantImage does not exist")]
    public async Task Handle_ShouldReturnFailure_WhenImageDoesNotExist()
    {
        var result = await _handler.Handle(
            new GetImage.Query(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when download fails")]
    public async Task Handle_ShouldReturnFailure_WhenDownloadFails()
    {
        var image = new VariantImage
        {
            Id = Guid.NewGuid(),
            FileName = "missing.jpg",
            ContentType = "image/jpeg",
            StoragePath = "images/missing.jpg",
            Url = string.Empty
        };
        _dbContext.Set<VariantImage>().Add(image);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _storageServiceMock
            .Setup(s => s.DownloadAsync(image.StoragePath, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DownloadResult>.NotFound("File not found"));

        var result = await _handler.Handle(
            new GetImage.Query(image.Id),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when storage download fails")]
    public async Task Handle_ShouldReturnFailure_WhenStorageDownloadFails()
    {
        var image = new VariantImage
        {
            Id = Guid.NewGuid(),
            FileName = "test.jpg",
            ContentType = "image/jpeg",
            StoragePath = "images/test.jpg",
            Url = string.Empty
        };
        _dbContext.Set<VariantImage>().Add(image);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _storageServiceMock
            .Setup(s => s.DownloadAsync(image.StoragePath, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DownloadResult>.NotFound("Path not found"));

        var result = await _handler.Handle(
            new GetImage.Query(image.Id),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
