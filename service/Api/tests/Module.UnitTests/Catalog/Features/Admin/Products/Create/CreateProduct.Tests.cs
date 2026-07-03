using MediatR;

using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Admin.Products.Create;
using Module.Catalog.Features.Admin.Products.Variants.Add;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductCreate")]
public class CreateProductTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<ILogger<CreateProduct.CommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly CreateProduct.CommandHandler _handler;

    public CreateProductTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _senderMock = new Mock<ISender>();
        _senderMock.Setup(x => x.Send(It.IsAny<AddVariant.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AddVariant.Response>.Ok(new AddVariant.Response { Id = Guid.NewGuid() }));

        _loggerMock = new Mock<ILogger<CreateProduct.CommandHandler>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _handler = new CreateProduct.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object, _senderMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create product and dispatch AddVariant")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var request = new CreateProduct.Request
        {
            Name = "T-Shirt",
            Slug = "t-shirt",
            Description = "A cotton t-shirt",
        };

        var result = await _handler.Handle(new CreateProduct.Command(request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("T-Shirt");
        result.Value.Slug.Should().Be("t-shirt");

        var persisted = await _dbContext.Set<Product>().FirstOrDefaultAsync(x => x.Slug == "t-shirt", cancellationToken: TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.MasterVariantId.Should().NotBe(Guid.Empty);

        _senderMock.Verify(x => x.Send(
            It.Is<AddVariant.Command>(c =>
                c.ProductId == persisted.Id &&
                c.Request.IsMaster),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handler: Should return failure when slug is duplicate")]
    public async Task Handle_ShouldReturnFailure_WhenSlugIsDuplicate()
    {
        var existing = ProductMethod.Create("Existing", "t-shirt", status: ProductStatus.Draft).Value;
        _dbContext.Set<Product>().Add(existing);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CreateProduct.Request
        {
            Name = "T-Shirt",
            Slug = "t-shirt",
        };

        var result = await _handler.Handle(new CreateProduct.Command(request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ProductResult.Errors.DuplicateSlug.Code);

        _senderMock.Verify(x => x.Send(It.IsAny<AddVariant.Command>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
