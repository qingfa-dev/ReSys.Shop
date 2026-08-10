using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Features.Admin.Variants.Images.GetById;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Images.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "VariantImageGetById")]
public class GetVariantImageByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetVariantImageById.QueryHandler _handler;

    public GetVariantImageByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(VariantImage).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetVariantImageById.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return image when found")]
    public async Task Handle_ShouldReturnImage_WhenFound()
    {
        var variantId = Guid.NewGuid();
        var image = Module.Catalog.Domain.Variants.Images.VariantImageMethod.Create(
            "image/jpeg", "photo.jpg", 2048,
            url: "https://cdn.test.com/photo.jpg",
            storagePath: "uploads/photo.jpg",
            position: 0, alt: "Photo", type: VariantImageType.Default,
            variantId: variantId).Value;

        _dbContext.Set<VariantImage>().Add(image);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetVariantImageById.Query(image.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(image.Id);
        result.Value.VariantId.Should().Be(variantId);
        result.Value.Url.Should().Be("https://cdn.test.com/photo.jpg");
        result.Value.FileName.Should().Be("photo.jpg");
        result.Value.ContentType.Should().Be("image/jpeg");
        result.Value.FileSize.Should().Be(2048);
        result.Value.Alt.Should().Be("Photo");
        result.Value.Type.Should().Be("Default");
    }

    [Fact(DisplayName = "Handler: Should return failure when image not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        var result = await _handler.Handle(
            new GetVariantImageById.Query(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(VariantImageResult.Failure.ById(Guid.Empty).Code);
    }
}
