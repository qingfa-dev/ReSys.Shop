using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Features.Admin.Variants.Images.Delete;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Images.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "VariantImageDelete")]
public class DeleteVariantImageTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly Mock<ILogger<DeleteVariantImage.CommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly DeleteVariantImage.CommandHandler _handler;

    public DeleteVariantImageTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(VariantImage).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _storageServiceMock = new Mock<IStorageService>();
        _loggerMock = new Mock<ILogger<DeleteVariantImage.CommandHandler>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _handler = new DeleteVariantImage.CommandHandler(
            _dbContext, _storageServiceMock.Object, _loggerMock.Object, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should delete image and remove from storage")]
    public async Task Handle_ShouldDeleteImage_WhenValid()
    {
        var image = Module.Catalog.Domain.Variants.Images.VariantImageMethod.Create("image/jpeg", "photo.jpg", 1024,
            url: "https://cdn.test.com/photo.jpg", storagePath: "uploads/photo.jpg",
            variantId: Guid.NewGuid()).Value;
        _dbContext.Set<VariantImage>().Add(image);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _storageServiceMock
            .Setup(x => x.DeleteAsync(image.StoragePath, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var result = await _handler.Handle(
            new DeleteVariantImage.Command(image.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var deleted = await _dbContext.Set<VariantImage>()
            .FirstOrDefaultAsync(x => x.Id == image.Id, TestContext.Current.CancellationToken);
        deleted.Should().BeNull();

        _storageServiceMock.Verify(
            x => x.DeleteAsync("uploads/photo.jpg", It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Handler: Should return failure when image not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        var result = await _handler.Handle(
            new DeleteVariantImage.Command(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(VariantImageResult.Failure.ById(Guid.Empty).Code);
    }

    [Fact(DisplayName = "Handler: Should propagate storage failure")]
    public async Task Handle_ShouldReturnFailure_WhenStorageFails()
    {
        var image = Module.Catalog.Domain.Variants.Images.VariantImageMethod.Create("image/jpeg", "photo.jpg", 1024,
            url: "https://cdn.test.com/photo.jpg", storagePath: "uploads/photo.jpg",
            variantId: Guid.NewGuid()).Value;
        _dbContext.Set<VariantImage>().Add(image);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _storageServiceMock
            .Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Unexpected("Storage.RemoveError", "Failed to remove"));

        var result = await _handler.Handle(
            new DeleteVariantImage.Command(image.Id),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Storage.RemoveError");
    }
}
