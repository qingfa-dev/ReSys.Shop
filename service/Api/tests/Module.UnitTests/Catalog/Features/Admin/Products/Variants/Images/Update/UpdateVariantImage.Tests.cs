
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Admin.Products.Update;

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
        var image = Module.Catalog.Domain.Products.Variants.Images.VariantImageMethod.Create("image/jpeg", "photo.jpg", 1024,
            url: "https://cdn.test.com/photo.jpg", storagePath: "u/photo.jpg",
            position: 0, alt: "Old alt", type: VariantImageType.Default).Value;
        _dbContext.Set<VariantImage>().Add(image);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateVariantImage.Request
        {
            Alt = "New alt text",
            Position = 3,
            Type = "Gallery"
        };

        var result = await _handler.Handle(
            new UpdateVariantImage.Command(image.Id, request),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Alt.Should().Be("New alt text");
        result.Value.Position.Should().Be(3);
        result.Value.Type.Should().Be("Gallery");

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
        var image = Module.Catalog.Domain.Products.Variants.Images.VariantImageMethod.Create("image/jpeg", "photo.jpg", 1024,
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
        result.Value.Type.Should().Be("Default");
    }
}
