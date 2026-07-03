using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Features.Storefront.Products.Get.Search;

namespace Module.UnitTests.Catalog.Features.Storefront.Products.Get.Search;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontSearch")]
public class SearchProductsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly SearchProducts.PagedQueryHandler _handler;

    public SearchProductsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new SearchProducts.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return matching products by name", Skip = "Requires PostgreSQL (ILike)")]
    public async Task Handle_ShouldReturnResults_WhenNameMatches()
    {
        var product = ProductMethod.Create("Blue T-Shirt", "blue-tshirt", status: ProductStatus.Active).Value;
        product.AvailableOn = DateTimeOffset.UtcNow.AddDays(-1);
        product.MasterVariantId = VariantMethod.Create(product.Id, "M", isMaster: true).Value.Id;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new SearchProducts.Query(new SearchProducts.Parameters { Q = "T-Shirt" }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Blue T-Shirt");
    }

    [Fact(DisplayName = "Handler: Should return empty when no match", Skip = "Requires PostgreSQL (ILike)")]
    public async Task Handle_ShouldReturnEmpty_WhenNoMatch()
    {
        var product = ProductMethod.Create("Shoes", "shoes", status: ProductStatus.Active).Value;
        product.AvailableOn = DateTimeOffset.UtcNow.AddDays(-1);
        product.MasterVariantId = VariantMethod.Create(product.Id, "M", isMaster: true).Value.Id;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new SearchProducts.Query(new SearchProducts.Parameters { Q = "NonExistent" }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }
}
