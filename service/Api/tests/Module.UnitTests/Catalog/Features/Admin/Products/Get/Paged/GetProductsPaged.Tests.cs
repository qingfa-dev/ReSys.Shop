using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Admin.Products.Get.Paged;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Get.Paged;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductGetPaged")]
public class GetProductsPagedTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetProductsPagedList.PagedQueryHandler _handler;

    public GetProductsPagedTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetProductsPagedList.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return empty list when no products exist")]
    public async Task Handle_ShouldReturnEmpty_WhenNoProducts()
    {
        var parameters = new GetProductsPagedList.Parameters();

        var result = await _handler.Handle(new GetProductsPagedList.Query(parameters), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact(DisplayName = "Handler: Should return products when they exist")]
    public async Task Handle_ShouldReturnProducts_WhenTheyExist()
    {
        var product1 = ProductMethod.Create("Product A", "product-a", status: ProductStatus.Active).Value;
        var product2 = ProductMethod.Create("Product B", "product-b", status: ProductStatus.Draft).Value;
        _dbContext.Set<Product>().AddRange(product1, product2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var parameters = new GetProductsPagedList.Parameters();

        var result = await _handler.Handle(new GetProductsPagedList.Query(parameters), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact(DisplayName = "Handler: Should filter products by status")]
    public async Task Handle_ShouldFilterByStatus()
    {
        var product1 = ProductMethod.Create("Active Product", "active-product", status: ProductStatus.Active).Value;
        var product2 = ProductMethod.Create("Draft Product", "draft-product", status: ProductStatus.Draft).Value;
        _dbContext.Set<Product>().AddRange(product1, product2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var parameters = new GetProductsPagedList.Parameters { Status = ProductStatus.Active };

        var result = await _handler.Handle(new GetProductsPagedList.Query(parameters), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Active Product");
    }

    [Fact(DisplayName = "Handler: Should exclude soft-deleted products")]
    public async Task Handle_ShouldExcludeSoftDeletedProducts()
    {
        var active = ProductMethod.Create("Active", "active", status: ProductStatus.Active).Value;
        var deleted = ProductMethod.Create("Deleted", "deleted", status: ProductStatus.Draft).Value;
        deleted.Delete("admin");
        _dbContext.Set<Product>().AddRange(active, deleted);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var parameters = new GetProductsPagedList.Parameters();

        var result = await _handler.Handle(new GetProductsPagedList.Query(parameters), TestContext.Current.CancellationToken);

        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Active");
    }

    // Note: Search filter uses EF.Functions.ILike (PostgreSQL-specific) and cannot be tested with InMemoryDatabase.
    // Search filter is covered by integration tests with a real PostgreSQL instance.
}
