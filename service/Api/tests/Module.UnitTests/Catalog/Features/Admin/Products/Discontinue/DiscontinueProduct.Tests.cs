using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Admin.Products.Discontinue;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Discontinue;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductDiscontinue")]
public class DiscontinueProductTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<DiscontinueProduct.CommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly DiscontinueProduct.CommandHandler _handler;

    public DiscontinueProductTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<DiscontinueProduct.CommandHandler>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _handler = new DiscontinueProduct.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should discontinue product and set DiscontinueOn")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new DiscontinueProduct.Command(product.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<Product>().FirstOrDefaultAsync(x => x.Id == product.Id, cancellationToken: TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.Status.Should().Be(ProductStatus.Archived);
        persisted.DiscontinueOn.Should().NotBeNull();
    }

    [Fact(DisplayName = "Handler: Should return failure when product not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        var result = await _handler.Handle(new DiscontinueProduct.Command(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ProductResult.Errors.NotFound(Guid.Empty).Code);
    }

    [Fact(DisplayName = "Handler: Should return success when product already archived")]
    public async Task Handle_ShouldReturnSuccess_WhenAlreadyArchived()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Archived).Value;
        product.DiscontinueOn = DateTimeOffset.UtcNow.AddDays(-1);
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new DiscontinueProduct.Command(product.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should set DiscontinueOn when null")]
    public async Task Handle_ShouldSetDiscontinueOn_WhenNull()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new DiscontinueProduct.Command(product.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var persisted = await _dbContext.Set<Product>().FirstOrDefaultAsync(x => x.Id == product.Id, cancellationToken: TestContext.Current.CancellationToken);
        persisted!.DiscontinueOn.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "Handler: Should preserve existing DiscontinueOn")]
    public async Task Handle_ShouldPreserveExistingDiscontinueOn()
    {
        var existingDate = DateTimeOffset.UtcNow.AddDays(-10);
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;
        product.DiscontinueOn = existingDate;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await _handler.Handle(new DiscontinueProduct.Command(product.Id), TestContext.Current.CancellationToken);

        var persisted = await _dbContext.Set<Product>().FirstOrDefaultAsync(x => x.Id == product.Id, cancellationToken: TestContext.Current.CancellationToken);
        persisted!.DiscontinueOn.Should().Be(existingDate);
    }
}
