using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Admin.Products.Variants.Images.Get.PagedOrAll;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Images.ListByVariant;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "VariantImageList")]
public class ListVariantImagesTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetVariantImagePagedOrAll.PagedQueryHandler _handler;

    public ListVariantImagesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(VariantImage).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetVariantImagePagedOrAll.PagedQueryHandler(_dbContext);
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
            new GetVariantImagePagedOrAll.Query(variantId, new GetVariantImagePagedOrAll.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(3);
        result.Items.First().Position.Should().Be(0);
        result.Items.First().FileName.Should().Be("second.png");
        result.Items.ElementAt(1).Position.Should().Be(1);
        result.Items.ElementAt(1).FileName.Should().Be("third.gif");
        result.Items.ElementAt(2).Position.Should().Be(2);
        result.Items.ElementAt(2).FileName.Should().Be("first.jpg");
    }

    [Fact(DisplayName = "Handler: Should return empty list when variant has no images")]
    public async Task Handle_ShouldReturnEmpty_WhenNoImages()
    {
        var result = await _handler.Handle(
            new GetVariantImagePagedOrAll.Query(Guid.NewGuid(), new GetVariantImagePagedOrAll.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should return images in pages when parameters supplied")]
    public async Task Handle_ShouldPage_WhenParametersSupplied()
    {
        var variantId = Guid.NewGuid();
        _dbContext.Set<VariantImage>().AddRange(
            Module.Catalog.Domain.Products.Variants.Images.VariantImageMethod.Create("image/jpeg", "first.jpg", 100,
                url: "https://cdn.test.com/1.jpg", storagePath: "u/1.jpg",
                position: 0, variantId: variantId).Value,
            Module.Catalog.Domain.Products.Variants.Images.VariantImageMethod.Create("image/png", "second.png", 200,
                url: "https://cdn.test.com/2.png", storagePath: "u/2.png",
                position: 1, variantId: variantId).Value,
            Module.Catalog.Domain.Products.Variants.Images.VariantImageMethod.Create("image/gif", "third.gif", 300,
                url: "https://cdn.test.com/3.gif", storagePath: "u/3.gif",
                position: 2, variantId: variantId).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetVariantImagePagedOrAll.Query(variantId, new GetVariantImagePagedOrAll.Parameters { PageSize = 2 }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
    }

    [Fact(DisplayName = "Handler: Should filter images by type when specified")]
    public async Task Handle_ShouldFilterByType_WhenSpecified()
    {
        var variantId = Guid.NewGuid();
        _dbContext.Set<VariantImage>().AddRange(
            Module.Catalog.Domain.Products.Variants.Images.VariantImageMethod.Create("image/jpeg", "default.jpg", 100,
                url: "https://cdn.test.com/1.jpg", storagePath: "u/1.jpg",
                position: 0, type: VariantImageType.Default, variantId: variantId).Value,
            Module.Catalog.Domain.Products.Variants.Images.VariantImageMethod.Create("image/png", "default.png", 200,
                url: "https://cdn.test.com/2.png", storagePath: "u/2.png",
                position: 1, type: VariantImageType.Default, variantId: variantId).Value,
            Module.Catalog.Domain.Products.Variants.Images.VariantImageMethod.Create("image/gif", "gallery.gif", 300,
                url: "https://cdn.test.com/3.gif", storagePath: "u/3.gif",
                position: 2, type: VariantImageType.Gallery, variantId: variantId).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetVariantImagePagedOrAll.Query(variantId, new GetVariantImagePagedOrAll.Parameters { Filter = "Type=Default" }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(i => i.FileName.Should().BeOneOf("default.jpg", "default.png"));
    }

    [Fact(DisplayName = "Handler: Should sort images by position descending when specified")]
    public async Task Handle_ShouldSortByPositionDescending_WhenSpecified()
    {
        var variantId = Guid.NewGuid();
        _dbContext.Set<VariantImage>().AddRange(
            Module.Catalog.Domain.Products.Variants.Images.VariantImageMethod.Create("image/jpeg", "first.jpg", 100,
                url: "https://cdn.test.com/1.jpg", storagePath: "u/1.jpg",
                position: 0, variantId: variantId).Value,
            Module.Catalog.Domain.Products.Variants.Images.VariantImageMethod.Create("image/png", "second.png", 200,
                url: "https://cdn.test.com/2.png", storagePath: "u/2.png",
                position: 1, variantId: variantId).Value,
            Module.Catalog.Domain.Products.Variants.Images.VariantImageMethod.Create("image/gif", "third.gif", 300,
                url: "https://cdn.test.com/3.gif", storagePath: "u/3.gif",
                position: 2, variantId: variantId).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetVariantImagePagedOrAll.Query(variantId, new GetVariantImagePagedOrAll.Parameters { Sort = ["Position:desc"] }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(3);
        result.Items.Should().BeInDescendingOrder(i => i.Position);
    }

    [Fact(DisplayName = "Handler: Should silently ignore disallowed filter field")]
    public async Task Handle_ShouldIgnoreDisallowedFilterField()
    {
        var variantId = Guid.NewGuid();
        _dbContext.Set<VariantImage>().AddRange(
            Module.Catalog.Domain.Products.Variants.Images.VariantImageMethod.Create("image/jpeg", "first.jpg", 100,
                url: "https://cdn.test.com/1.jpg", storagePath: "u/1.jpg",
                position: 0, variantId: variantId).Value,
            Module.Catalog.Domain.Products.Variants.Images.VariantImageMethod.Create("image/png", "second.png", 200,
                url: "https://cdn.test.com/2.png", storagePath: "u/2.png",
                position: 1, variantId: variantId).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetVariantImagePagedOrAll.Query(variantId, new GetVariantImagePagedOrAll.Parameters { Filter = "NonExistent=1" }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
    }
}
