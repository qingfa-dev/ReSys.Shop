using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Features.Admin.Products.Variants.Add;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Add;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "VariantAdd")]
public class AddVariantTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<AddVariant.CommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly AddVariant.CommandHandler _handler;

    public AddVariantTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<AddVariant.CommandHandler>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _handler = new AddVariant.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create variant successfully")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Draft).Value;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new AddVariant.Request
        {
            Sku = "SKU-001",
            IsMaster = false,
            Position = 1,
        };

        var result = await _handler.Handle(new AddVariant.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Sku.Should().Be("SKU-001");

        var persisted = await _dbContext.Set<Variant>().FirstOrDefaultAsync(x => x.Sku == "SKU-001", cancellationToken: TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.ProductId.Should().Be(product.Id);
        persisted.IsMaster.Should().BeFalse();
        persisted.Position.Should().Be(1);
    }

    [Fact(DisplayName = "Handler: Should return failure when product not found")]
    public async Task Handle_ShouldReturnFailure_WhenProductNotFound()
    {
        var request = new AddVariant.Request { Sku = "SKU-001" };

        var result = await _handler.Handle(new AddVariant.Command(Guid.NewGuid(), request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ProductResult.Errors.NotFound(Guid.Empty).Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when master variant has option values")]
    public async Task Handle_ShouldReturnFailure_WhenMasterHasOptionValues()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Draft).Value;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new AddVariant.Request
        {
            Sku = "MASTER-SKU",
            IsMaster = true,
            OptionValueIds = [Guid.NewGuid()],
        };

        var result = await _handler.Handle(new AddVariant.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(VariantResult.Errors.MasterCannotHaveOptions.Code);
    }
}
