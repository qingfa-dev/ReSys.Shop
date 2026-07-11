using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Admin.Products.Variants.Images.ListByVariant;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Images.ListByVariant;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "VariantImageList")]
public class ListVariantImagesTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ListVariantImages.QueryHandler _handler;

    public ListVariantImagesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(VariantImage).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new ListVariantImages.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return images ordered by position")]
    public async Task Handle_ShouldReturnImagesOrderedByPosition()
    {
        var variantId = Guid.NewGuid();
        var img1 = Module.Catalog.Domain.Products.Variants.Images.VariantImageMethod.Create("image/jpeg", "first.jpg", 100,
            url: "https://cdn.test.com/1.jpg", storagePath: "u/1.jpg",
            position: 2, variantId: variantId).Value;
        var img2 = Module.Catalog.Domain.Products.Variants.Images.VariantImageMethod.Create("image/png", "second.png", 200,
            url: "https://cdn.test.com/2.png", storagePath: "u/2.png",
            position: 0, variantId: variantId).Value;
        var img3 = Module.Catalog.Domain.Products.Variants.Images.VariantImageMethod.Create("image/gif", "third.gif", 300,
            url: "https://cdn.test.com/3.gif", storagePath: "u/3.gif",
            position: 1, variantId: variantId).Value;

        _dbContext.Set<VariantImage>().AddRange(img1, img2, img3);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new ListVariantImages.Query(variantId),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Images.Should().HaveCount(3);
        result.Value.Images[0].Position.Should().Be(0);
        result.Value.Images[0].FileName.Should().Be("second.png");
        result.Value.Images[1].Position.Should().Be(1);
        result.Value.Images[1].FileName.Should().Be("third.gif");
        result.Value.Images[2].Position.Should().Be(2);
        result.Value.Images[2].FileName.Should().Be("first.jpg");
    }

    [Fact(DisplayName = "Handler: Should return empty list when variant has no images")]
    public async Task Handle_ShouldReturnEmpty_WhenNoImages()
    {
        var result = await _handler.Handle(
            new ListVariantImages.Query(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Images.Should().BeEmpty();
    }
}
