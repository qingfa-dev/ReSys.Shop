using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Options;
using Module.Catalog.Domain.Products.Variants.Prices;
using Module.Catalog.Features.Storefront.Products.Get.Filter;

namespace Module.UnitTests.Catalog.Features.Storefront.Products.Get.Filter;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontFilter")]
public class FilterProductsByAttributesTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly FilterProducts.PagedQueryHandler _handler;

    public FilterProductsByAttributesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new FilterProducts.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should filter products by color", Skip = "Requires PostgreSQL (ILike)")]
    public async Task Handle_ShouldFilterByColor()
    {
        var colorType = new OptionType { Name = "Color", Filterable = true };
        var red = new OptionValue { Name = "Red", OptionType = colorType };

        var product = ProductMethod.Create("Red Shirt", "red-shirt", status: ProductStatus.Active).Value;
        product.AvailableOn = DateTimeOffset.UtcNow.AddDays(-1);
        var variant = VariantMethod.Create(product.Id, "R-M", isMaster: false).Value;
        variant.Prices.Add(new Price { Amount = 10m, Currency = "USD" });
        variant.OptionValueVariants.Add(new OptionValueVariant { Variant = variant, VariantId = variant.Id, OptionValue = red, OptionValueId = red.Id });
        product.Variants.Add(variant);
        product.MasterVariantId = VariantMethod.Create(product.Id, "M", isMaster: true).Value.Id;

        _dbContext.Set<OptionType>().Add(colorType);
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new FilterProducts.Query(new FilterProducts.Parameters { Color = "Red" }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Handler: Should return empty when no products match color")]
    public async Task Handle_ShouldReturnEmpty_WhenColorNoMatch()
    {
        var product = ProductMethod.Create("Blue Shirt", "blue-shirt", status: ProductStatus.Active).Value;
        product.AvailableOn = DateTimeOffset.UtcNow.AddDays(-1);
        product.MasterVariantId = VariantMethod.Create(product.Id, "M", isMaster: true).Value.Id;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new FilterProducts.Query(new FilterProducts.Parameters { Color = "NonExistent" }),
            TestContext.Current.CancellationToken);

        result.Items.Should().BeEmpty();
    }
}
