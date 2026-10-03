using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Variants;
using Module.Catalog.Features.Admin.Products.Delete;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductDelete")]
public class DeleteProductTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<DeleteProduct.CommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly DeleteProduct.CommandHandler _handler;

    public DeleteProductTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<DeleteProduct.CommandHandler>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _handler = new DeleteProduct.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should soft-delete product and its variants")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Draft).Value;
        var variant1 = VariantMethod.Create(product.Id, "SKU-001", isMaster: true).Value;
        var variant2 = VariantMethod.Create(product.Id, "SKU-002", isMaster: false).Value;
        product.Variants.Add(variant1);
        product.Variants.Add(variant2);
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new DeleteProduct.Command(product.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var deletedProduct = await _dbContext.Set<Product>().IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == product.Id, cancellationToken: TestContext.Current.CancellationToken);
        deletedProduct.Should().NotBeNull();
        deletedProduct!.IsDeleted.Should().BeTrue();

        var deletedVariants = await _dbContext.Set<Variant>().IgnoreQueryFilters().Where(x => x.ProductId == product.Id).ToListAsync(cancellationToken: TestContext.Current.CancellationToken);
        deletedVariants.Should().HaveCount(2);
        deletedVariants.Should().AllSatisfy(v => v.IsDeleted.Should().BeTrue());
    }

    [Fact(DisplayName = "Handler: Should return failure when product not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        var result = await _handler.Handle(new DeleteProduct.Command(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ProductResult.Errors.NotFound(Guid.Empty).Code);
    }

    [Fact(DisplayName = "Handler: Should return not-found when product already deleted (soft-delete filter applies)")]
    public async Task Handle_ShouldReturnNotFound_WhenAlreadyDeleted()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Draft).Value;
        product.IsDeleted = true;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new DeleteProduct.Command(product.Id), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ProductResult.Errors.NotFound(product.Id).Code);
    }

    [Fact(DisplayName = "Handler: Should set DeletedAtUtc and DeletedBy fields")]
    public async Task Handle_ShouldSetDeletedFields()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Draft).Value;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await _handler.Handle(new DeleteProduct.Command(product.Id), TestContext.Current.CancellationToken);

        var deleted = await _dbContext.Set<Product>().IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == product.Id, cancellationToken: TestContext.Current.CancellationToken);
        deleted.Should().NotBeNull();
        deleted!.DeletedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        deleted.DeletedBy.Should().Be("admin");
    }
}
