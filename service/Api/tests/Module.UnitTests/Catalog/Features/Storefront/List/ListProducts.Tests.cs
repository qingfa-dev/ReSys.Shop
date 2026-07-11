using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Prices;
using Module.Catalog.Features.Storefront.Products.Get.List;

namespace Module.UnitTests.Catalog.Features.Storefront.List;

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

    private static Product CreateActiveProduct(string name, string slug, DateTimeOffset? availableOn = null)
    {
        var product = ProductMethod.Create(name, slug, status: ProductStatus.Active).Value;
        product.AvailableOn = availableOn ?? DateTimeOffset.UtcNow.AddDays(-1);
        product.MasterVariantId = VariantMethod.Create(product.Id, "M", isMaster: true).Value.Id;
        return product;
    }

    [Fact(DisplayName = "Handler: Should return all active products with empty parameters")]
    public async Task Handle_ShouldReturnAllActiveProducts_WhenNoFilters()
    {
        var product = CreateActiveProduct("Blue T-Shirt", "blue-tshirt");
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
        var product = CreateActiveProduct("Future Item", "future-item", DateTimeOffset.UtcNow.AddDays(7));
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

    [Fact(DisplayName = "Handler: Should return empty products when alias filters set (InMemory skips ILike)")]
    public async Task Handle_ShouldReturnEmpty_WhenAliasFilterSet_OnInMemory()
    {
        // Note: Alias predicates (option_value, option_type, taxon) use EF.Functions.ILike
        // which is PostgreSQL-specific and cannot be translated by the InMemory provider.
        // Filter behavior is covered by integration tests with a real PostgreSQL instance.
        var result = await _handler.Handle(
            new ListProducts.Query(new ListProducts.Parameters { OptionValue = "Red" }),
            TestContext.Current.CancellationToken);

        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should apply price range via min_price/max_price aliases")]
    public async Task Handle_ShouldApplyPriceRangeViaMinMaxPriceAliases()
    {
        var cheapProduct = CreateActiveProduct("Cheap Item", "cheap-item");
        var cheapVariant = VariantMethod.Create(cheapProduct.Id, "M").Value;
        cheapVariant.Prices.Add(PriceMethod.Create(5m, "USD", cheapVariant.Id).Value);
        cheapProduct.Variants.Add(cheapVariant);
        _dbContext.Set<Variant>().Add(cheapVariant);
        _dbContext.Set<Product>().Add(cheapProduct);

        var priceyProduct = CreateActiveProduct("Pricey Item", "pricey-item");
        var priceyVariant = VariantMethod.Create(priceyProduct.Id, "M").Value;
        priceyVariant.Prices.Add(PriceMethod.Create(50m, "USD", priceyVariant.Id).Value);
        priceyProduct.Variants.Add(priceyVariant);
        _dbContext.Set<Variant>().Add(priceyVariant);
        _dbContext.Set<Product>().Add(priceyProduct);

        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new ListProducts.Query(new ListProducts.Parameters { MinPrice = 10, MaxPrice = 40 }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should silently ignore unwhitelisted raw filter field")]
    public async Task Handle_ShouldSilentlyIgnoreUnwhitelistedRawFilterField()
    {
        var product = CreateActiveProduct("Unfiltered Product", "unfiltered-product");
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new ListProducts.Query(new ListProducts.Parameters
            {
                Filter = "SomeSecretProperty=value"
            }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Unfiltered Product");
    }

    [Fact(DisplayName = "Alias map: BuildFilter returns empty string when no alias is set")]
    public void BuildFilter_ShouldReturnEmpty_WhenNoAliasSet()
    {
        var result = StorefrontProductFilterAliases.BuildFilter(new ListProducts.Parameters());

        result.Should().Be(string.Empty);
    }

    [Fact(DisplayName = "Alias map: BuildFilter wraps string aliases in *value*")]
    public void BuildFilter_ShouldWrapStringAliasesInContainsOperator()
    {
        var result = StorefrontProductFilterAliases.BuildFilter(
            new ListProducts.Parameters { OptionValue = "Red" });

        result.Should().Be("Variants.OptionValueVariants.OptionValue.Name=*Red*");
    }

    [Fact(DisplayName = "Alias map: BuildFilter emits two conditions for min/max price")]
    public void BuildFilter_ShouldEmitTwoConditions_ForMinMaxPrice()
    {
        var result = StorefrontProductFilterAliases.BuildFilter(
            new ListProducts.Parameters { MinPrice = 10, MaxPrice = 50 });

        result.Should().Be("Variants.Prices.Amount>=10,Variants.Prices.Amount<=50");
    }
}
