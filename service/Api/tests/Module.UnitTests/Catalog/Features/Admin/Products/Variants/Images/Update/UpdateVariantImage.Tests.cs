
using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Features.Admin.Variants.Images.Update;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Images.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "VariantImageUpdate")]
public class UpdateVariantImageTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<UpdateVariantImage.CommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly UpdateVariantImage.CommandHandler _handler;

    public UpdateVariantImageTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(VariantImage).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<UpdateVariantImage.CommandHandler>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _handler = new UpdateVariantImage.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should update image alt, position, and type")]
    public async Task Handle_ShouldUpdateFields_WhenValid()
    {
        var image = Module.Catalog.Domain.Variants.Images.VariantImageMethod.Create("image/jpeg", "photo.jpg", 1024,
            url: "https://cdn.test.com/photo.jpg", storagePath: "u/photo.jpg",
            position: 0, alt: "Old alt", type: VariantImageType.Default).Value;
        _dbContext.Set<VariantImage>().Add(image);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateVariantImage.Request
        {
            Alt = "New alt text",
            Position = 3,
            Type = VariantImageType.Gallery
        };

        var result = await _handler.Handle(
            new UpdateVariantImage.Command(image.Id, request),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Alt.Should().Be("New alt text");
        result.Value.Position.Should().Be(3);
        result.Value.Type.Should().Be(VariantImageType.Gallery);

        var persisted = await _dbContext.Set<VariantImage>()
            .FirstAsync(x => x.Id == image.Id, TestContext.Current.CancellationToken);
        persisted.Alt.Should().Be("New alt text");
        persisted.Position.Should().Be(3);
        persisted.Type.Should().Be(VariantImageType.Gallery);
    }

    [Fact(DisplayName = "Handler: Should return failure when image not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        var request = new UpdateVariantImage.Request { Alt = "New alt" };
        var result = await _handler.Handle(
            new UpdateVariantImage.Command(Guid.NewGuid(), request),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(VariantImageResult.Failure.ById(Guid.Empty).Code);
    }

    [Fact(DisplayName = "Handler: Should preserve unset fields when update only changes position")]
    public async Task Handle_ShouldPreserveUnsetFields_WhenPartialUpdate()
    {
        var image = Module.Catalog.Domain.Variants.Images.VariantImageMethod.Create("image/jpeg", "photo.jpg", 1024,
            url: "https://cdn.test.com/photo.jpg", storagePath: "u/photo.jpg",
            position: 0, alt: "Original alt", type: VariantImageType.Default).Value;
        _dbContext.Set<VariantImage>().Add(image);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateVariantImage.Request { Position = 5 };

        var result = await _handler.Handle(
            new UpdateVariantImage.Command(image.Id, request),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Position.Should().Be(5);
        result.Value.Alt.Should().Be("Original alt");
        result.Value.Type.Should().Be(VariantImageType.Default);
    }

    [Fact(DisplayName = "Handler: Should preserve the existing type when Type is not provided (null)")]
    public async Task Handle_ShouldPreserveType_WhenTypeNotProvided()
    {
        var variantId = Guid.NewGuid();
        var existing = Module.Catalog.Domain.Variants.Images.VariantImageMethod.Create("image/jpeg", "old.jpg", 1024,
            url: "https://cdn.test.com/old.jpg", storagePath: "u/old.jpg",
            position: 0, type: VariantImageType.Default, variantId: variantId).Value;
        var image = Module.Catalog.Domain.Variants.Images.VariantImageMethod.Create("image/jpeg", "new.jpg", 1024,
            url: "https://cdn.test.com/new.jpg", storagePath: "u/new.jpg",
            position: 1, type: VariantImageType.Gallery, variantId: variantId).Value;
        _dbContext.Set<VariantImage>().AddRange(existing, image);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateVariantImage.Request { Position = 2 };
        var result = await _handler.Handle(
            new UpdateVariantImage.Command(image.Id, request),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(VariantImageType.Gallery);

        var demoted = await _dbContext.Set<VariantImage>()
            .FirstAsync(x => x.Id == existing.Id, TestContext.Current.CancellationToken);
        demoted.Type.Should().Be(VariantImageType.Default);
    }

    [Fact(DisplayName = "Handler: Should demote to Default when Type is explicitly Default")]
    public async Task Handle_ShouldDemoteToDefault_WhenTypeIsDefault()
    {
        var variantId = Guid.NewGuid();
        var existing = Module.Catalog.Domain.Variants.Images.VariantImageMethod.Create("image/jpeg", "old.jpg", 1024,
            url: "https://cdn.test.com/old.jpg", storagePath: "u/old.jpg",
            position: 0, type: VariantImageType.Default, variantId: variantId).Value;
        var image = Module.Catalog.Domain.Variants.Images.VariantImageMethod.Create("image/jpeg", "search.jpg", 1024,
            url: "https://cdn.test.com/search.jpg", storagePath: "u/search.jpg",
            position: 1, type: VariantImageType.Search, variantId: variantId).Value;
        _dbContext.Set<VariantImage>().AddRange(existing, image);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateVariantImage.Request { Type = VariantImageType.Default };
        var result = await _handler.Handle(
            new UpdateVariantImage.Command(image.Id, request),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(VariantImageType.Default);

        var demoted = await _dbContext.Set<VariantImage>()
            .FirstAsync(x => x.Id == existing.Id, TestContext.Current.CancellationToken);
        demoted.Type.Should().Be(VariantImageType.Thumbnail);
    }

    [Fact(DisplayName = "Handler: Should demote the prior Search image when setting a new Search")]
    public async Task Handle_ShouldDemotePriorSearch_WhenSettingNewSearch()
    {
        var variantId = Guid.NewGuid();
        var existing = Module.Catalog.Domain.Variants.Images.VariantImageMethod.Create("image/jpeg", "old.jpg", 1024,
            url: "https://cdn.test.com/old.jpg", storagePath: "u/old.jpg",
            position: 0, type: VariantImageType.Search, variantId: variantId).Value;
        var image = Module.Catalog.Domain.Variants.Images.VariantImageMethod.Create("image/jpeg", "new.jpg", 1024,
            url: "https://cdn.test.com/new.jpg", storagePath: "u/new.jpg",
            position: 1, type: VariantImageType.Gallery, variantId: variantId).Value;
        _dbContext.Set<VariantImage>().AddRange(existing, image);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateVariantImage.Request { Type = VariantImageType.Search };
        var result = await _handler.Handle(
            new UpdateVariantImage.Command(image.Id, request),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(VariantImageType.Search);

        var demoted = await _dbContext.Set<VariantImage>()
            .FirstAsync(x => x.Id == existing.Id, TestContext.Current.CancellationToken);
        demoted.Type.Should().Be(VariantImageType.Thumbnail);
    }
}
