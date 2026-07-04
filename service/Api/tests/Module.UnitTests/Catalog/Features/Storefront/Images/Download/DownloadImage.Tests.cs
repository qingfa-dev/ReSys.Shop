using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Storefront.Images.Get.Download;

using Shared.Operational.Storages.Models;
using Shared.Application.Models.Results;
using Shared.Operational.Storages.Services;

using Moq;

namespace Module.UnitTests.Catalog.Features.Storefront.Images.Get.Download;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontDownloadImage")]
public class DownloadImageTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly DownloadImage.QueryHandler _handler;

    public DownloadImageTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _storageServiceMock = new Mock<IStorageService>();

        _handler = new DownloadImage.QueryHandler(_dbContext, _storageServiceMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return file stream when VariantImage exists")]
    public async Task Handle_ShouldReturnStream_WhenImageExists()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        var storedInfo = new StoredObjectInfo("images/test.jpg", "local", 3, DateTimeOffset.UtcNow, "image/jpeg");
        var image = new VariantImage
        {
            Id = Guid.NewGuid(),
            FileName = "test.jpg",
            ContentType = "image/jpeg",
            StoragePath = "images/test.jpg",
            Url = "/media/test.jpg"
        };
        _dbContext.Set<VariantImage>().Add(image);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _storageServiceMock
            .Setup(s => s.DownloadAsync(image.StoragePath, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DownloadResult>.Ok(new DownloadResult(stream, storedInfo)));

        var result = await _handler.Handle(
            new DownloadImage.Query(image.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.FileName.Should().Be("test.jpg");
        result.Value.ContentType.Should().Be("image/jpeg");
    }

    [Fact(DisplayName = "Handler: Should return failure when VariantImage does not exist")]
    public async Task Handle_ShouldReturnFailure_WhenImageDoesNotExist()
    {
        var result = await _handler.Handle(
            new DownloadImage.Query(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
