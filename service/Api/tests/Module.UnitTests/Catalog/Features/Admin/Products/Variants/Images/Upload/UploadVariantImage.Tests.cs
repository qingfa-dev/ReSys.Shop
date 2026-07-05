using Hangfire;

using Microsoft.AspNetCore.Http;

using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Admin.Products.Variants.Images.Upload;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Images.Upload;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "VariantImageUpload")]
public class UploadVariantImageTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly Mock<ILogger<UploadVariantImage.CommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly UploadVariantImage.CommandHandler _handler;

    public UploadVariantImageTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _storageServiceMock = new Mock<IStorageService>();
        _loggerMock = new Mock<ILogger<UploadVariantImage.CommandHandler>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _handler = new UploadVariantImage.CommandHandler(
            _dbContext, _storageServiceMock.Object, backgroundJobClient: null, _loggerMock.Object, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should upload image and create VariantImage entity")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var product = ProductMethod.Create("Test Product", "test-product", status: ProductStatus.Draft).Value;
        var variant = VariantMethod.Create(product.Id, "SKU-001", isMaster: true).Value;
        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Variant>().Add(variant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var uploadResult = Result<UploadResult>.Ok(new UploadResult(
            Key: "catalog/variants/1/images/img.jpg",
            Provider: "local",
            Uri: new Uri("https://cdn.test.com/media/img.jpg"),
            SizeBytes: 2048,
            StoredAtUtc: DateTimeOffset.UtcNow));

        _storageServiceMock
            .Setup(x => x.UploadAsync(
                It.IsAny<UploadRequest>(),
                It.IsAny<string?>(),
                It.IsAny<UploadOptions?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(uploadResult);

        var file = new FormFile(new MemoryStream(new byte[2048]), 0, 2048, "file", "img.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        var request = new UploadVariantImage.Request
        {
            File = file,
            Alt = "Test image",
            Position = 1,
            Type = "Gallery"
        };

        var result = await _handler.Handle(
            new UploadVariantImage.Command(variant.Id, request),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Url.Should().Be("https://cdn.test.com/media/img.jpg");
        result.Value.FileName.Should().Be("img.jpg");
        result.Value.ContentType.Should().Be("image/jpeg");
        result.Value.FileSize.Should().Be(2048);
        result.Value.Alt.Should().Be("Test image");
        result.Value.Position.Should().Be(1);
        result.Value.Type.Should().Be("Gallery");

        var persisted = await _dbContext.Set<VariantImage>()
            .FirstOrDefaultAsync(x => x.Id == result.Value.Id, TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.VariantId.Should().Be(variant.Id);
        persisted.StoragePath.Should().Be("catalog/variants/1/images/img.jpg");
    }

    [Fact(DisplayName = "Handler: Should return failure when variant not found")]
    public async Task Handle_ShouldReturnFailure_WhenVariantNotFound()
    {
        var file = new FormFile(new MemoryStream(new byte[1024]), 0, 1024, "file", "img.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        var request = new UploadVariantImage.Request { File = file };
        var result = await _handler.Handle(
            new UploadVariantImage.Command(Guid.NewGuid(), request),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(VariantResult.Errors.NotFound(Guid.Empty).Code);
    }

    [Fact(DisplayName = "Handler: Should propagate storage service failure")]
    public async Task Handle_ShouldReturnFailure_WhenStorageFails()
    {
        var product = ProductMethod.Create("Test", "test", status: ProductStatus.Draft).Value;
        var variant = VariantMethod.Create(product.Id, "SKU-001", isMaster: true).Value;
        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Variant>().Add(variant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _storageServiceMock
            .Setup(x => x.UploadAsync(
                It.IsAny<UploadRequest>(),
                It.IsAny<string?>(),
                It.IsAny<UploadOptions?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UploadResult>.Unexpected(errors: [Error.Unexpected("Storage.Error", "Storage failed")]));

        var file = new FormFile(new MemoryStream(new byte[1024]), 0, 1024, "file", "img.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        var request = new UploadVariantImage.Request { File = file };
        var result = await _handler.Handle(
            new UploadVariantImage.Command(variant.Id, request),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Storage.Error");
    }
}
