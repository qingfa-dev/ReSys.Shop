using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Storefront.Images.Get.Image;

using Shared.Operational.Storages.Services;

using Moq;

namespace Module.UnitTests.Catalog.Features.Storefront.Images.Get.Image;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontGetImage")]
public class GetImageTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly string _tempDir;
    private readonly GetImage.QueryHandler _handler;

    public GetImageTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _tempDir = Path.Combine(Path.GetTempPath(), $"imgtest-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        _storageServiceMock = new Mock<IStorageService>();

        _handler = new GetImage.QueryHandler(_dbContext, _storageServiceMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return full path when VariantImage exists and file is on disk")]
    public async Task Handle_ShouldReturnFullPath_WhenImageAndFileExist()
    {
        var fileName = "test.jpg";
        var storagePath = $"images/{fileName}";
        var fileDir = Path.Combine(_tempDir, "images");
        var filePath = Path.Combine(fileDir, fileName);
        Directory.CreateDirectory(fileDir);
        await File.WriteAllTextAsync(filePath, "fake-image-data", TestContext.Current.CancellationToken);

        var image = new VariantImage
        {
            Id = Guid.NewGuid(),
            FileName = "test.jpg",
            ContentType = "image/jpeg",
            StoragePath = storagePath,
            Url = string.Empty
        };
        _dbContext.Set<VariantImage>().Add(image);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _storageServiceMock
            .Setup(s => s.ResolvePathAsync(storagePath, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Ok(filePath));

        var result = await _handler.Handle(
            new GetImage.Query(image.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.FullPath.Should().Be(filePath);
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

    [Fact(DisplayName = "Handler: Should return failure when file does not exist on disk")]
    public async Task Handle_ShouldReturnFailure_WhenFileDoesNotExist()
    {
        var storagePath = "images/missing.jpg";
        var missingPath = Path.Combine(_tempDir, "images", "missing.jpg");

        var image = new VariantImage
        {
            Id = Guid.NewGuid(),
            FileName = "missing.jpg",
            ContentType = "image/jpeg",
            StoragePath = storagePath,
            Url = string.Empty
        };
        _dbContext.Set<VariantImage>().Add(image);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _storageServiceMock
            .Setup(s => s.ResolvePathAsync(storagePath, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Ok(missingPath));

        var result = await _handler.Handle(
            new GetImage.Query(image.Id),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when storage resolves to error")]
    public async Task Handle_ShouldReturnFailure_WhenResolvePathFails()
    {
        var storagePath = "images/test.jpg";

        var image = new VariantImage
        {
            Id = Guid.NewGuid(),
            FileName = "test.jpg",
            ContentType = "image/jpeg",
            StoragePath = storagePath,
            Url = string.Empty
        };
        _dbContext.Set<VariantImage>().Add(image);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _storageServiceMock
            .Setup(s => s.ResolvePathAsync(storagePath, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.NotFound("Path not found"));

        var result = await _handler.Handle(
            new GetImage.Query(image.Id),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
