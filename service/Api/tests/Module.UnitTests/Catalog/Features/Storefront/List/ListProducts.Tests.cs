using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Features.Storefront.Products.Get.List;

namespace Module.UnitTests.Catalog.Features.Storefront.Products.Get.List;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontListProducts")]
public class ListProductsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ListProducts.PagedQueryHandler _handler;

    public ListProductsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new ListProducts.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return all active products with empty parameters")]
    public async Task Handle_ShouldReturnAllActiveProducts_WhenNoFilters()
    {
        var product = ProductMethod.Create("Blue T-Shirt", "blue-tshirt", status: ProductStatus.Active).Value;
        product.AvailableOn = DateTimeOffset.UtcNow.AddDays(-1);
        product.MasterVariantId = VariantMethod.Create(product.Id, "M", isMaster: true).Value.Id;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new ListProducts.Query(new ListProducts.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Blue T-Shirt");
    }

    [Fact(DisplayName = "Handler: Should exclude discontinued products")]
    public async Task Handle_ShouldExcludeDiscontinuedProducts()
    {
        var product = ProductMethod.Create("Shoes", "shoes", status: ProductStatus.Archived).Value;
        product.MasterVariantId = VariantMethod.Create(product.Id, "M", isMaster: true).Value.Id;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new ListProducts.Query(new ListProducts.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should exclude future products")]
    public async Task Handle_ShouldExcludeFutureProducts()
    {
        var product = ProductMethod.Create("Future Item", "future-item", status: ProductStatus.Active).Value;
        product.AvailableOn = DateTimeOffset.UtcNow.AddDays(7);
        product.MasterVariantId = VariantMethod.Create(product.Id, "M", isMaster: true).Value.Id;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new ListProducts.Query(new ListProducts.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should return empty when no parameters and no products exist")]
    public async Task Handle_ShouldReturnEmpty_WhenNoProducts()
    {
        var result = await _handler.Handle(
            new ListProducts.Query(new ListProducts.Parameters()),
            TestContext.Current.CancellationToken);

        result.Items.Should().BeEmpty();
    }
}
