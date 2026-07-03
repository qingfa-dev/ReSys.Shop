using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Admin.Products.Activate;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Activate;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductActivate")]
public class ActivateProductTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<ActivateProduct.CommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly ActivateProduct.CommandHandler _handler;

    public ActivateProductTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<ActivateProduct.CommandHandler>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _handler = new ActivateProduct.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should activate product and set AvailableOn")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Draft).Value;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new ActivateProduct.Command(product.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<Product>().FirstOrDefaultAsync(x => x.Id == product.Id, cancellationToken: TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.Status.Should().Be(ProductStatus.Active);
        persisted.AvailableOn.Should().NotBeNull();
    }

    [Fact(DisplayName = "Handler: Should return failure when product not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        var result = await _handler.Handle(new ActivateProduct.Command(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ProductResult.Errors.NotFound(Guid.Empty).Code);
    }

    [Fact(DisplayName = "Handler: Should return success when product already active")]
    public async Task Handle_ShouldReturnSuccess_WhenAlreadyActive()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;
        product.AvailableOn = DateTimeOffset.UtcNow.AddDays(-1);
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new ActivateProduct.Command(product.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should set AvailableOn when null")]
    public async Task Handle_ShouldSetAvailableOn_WhenNull()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Draft).Value;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new ActivateProduct.Command(product.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var persisted = await _dbContext.Set<Product>().FirstOrDefaultAsync(x => x.Id == product.Id, cancellationToken: TestContext.Current.CancellationToken);
        persisted!.AvailableOn.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "Handler: Should preserve existing AvailableOn")]
    public async Task Handle_ShouldPreserveExistingAvailableOn()
    {
        var existingDate = DateTimeOffset.UtcNow.AddDays(-10);
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Draft).Value;
        product.AvailableOn = existingDate;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await _handler.Handle(new ActivateProduct.Command(product.Id), TestContext.Current.CancellationToken);

        var persisted = await _dbContext.Set<Product>().FirstOrDefaultAsync(x => x.Id == product.Id, cancellationToken: TestContext.Current.CancellationToken);
        persisted!.AvailableOn.Should().Be(existingDate);
    }
}
