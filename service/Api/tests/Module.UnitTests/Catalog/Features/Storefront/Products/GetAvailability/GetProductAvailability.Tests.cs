using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Options;
using Module.Catalog.Domain.Products.Variants.Prices;
using Module.Catalog.Features.Storefront.Products.Get.Availability;

namespace Module.UnitTests.Catalog.Features.Storefront.Products.Get.Availability;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontProductAvailability")]
public class GetProductAvailabilityTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetAvailability.QueryHandler _handler;

    public GetProductAvailabilityTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetAvailability.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return availability matrix for product with variants")]
    public async Task Handle_ShouldReturnMatrix_WhenProductHasVariants()
    {
        var product = ProductMethod.Create("Test Product", "test-product", status: ProductStatus.Active).Value;
        var colorType = new OptionType { Name = "Color", Presentation = "Color", Position = 1 };
        var sizeType = new OptionType { Name = "Size", Presentation = "Size", Position = 2 };
        var red = new OptionValue { Name = "Red", OptionType = colorType, Position = 1 };
        var blue = new OptionValue { Name = "Blue", OptionType = colorType, Position = 2 };
        var small = new OptionValue { Name = "S", OptionType = sizeType, Position = 1 };

        var variant1 = VariantMethod.Create(product.Id, "V-RED-S", isMaster: false).Value;
        variant1.OptionValueVariants.Add(new OptionValueVariant { VariantId = variant1.Id, OptionValueId = red.Id, OptionValue = red });
        variant1.OptionValueVariants.Add(new OptionValueVariant { VariantId = variant1.Id, OptionValueId = small.Id, OptionValue = small });
        variant1.Prices.Add(new Price { Amount = 19.99m, Currency = "USD" });

        product.Variants.Add(variant1);
        product.MasterVariantId = VariantMethod.Create(product.Id, "MASTER", isMaster: true).Value.Id;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetAvailability.Query(product.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Axes.Should().NotBeEmpty();
        result.Value.Cells.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "Handler: Should return empty matrix when no variants exist")]
    public async Task Handle_ShouldReturnEmptyMatrix_WhenNoVariants()
    {
        var product = ProductMethod.Create("Empty Product", "empty", status: ProductStatus.Active).Value;
        product.MasterVariantId = VariantMethod.Create(product.Id, "MASTER", isMaster: true).Value.Id;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetAvailability.Query(product.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Axes.Should().BeEmpty();
        result.Value.Cells.Should().BeEmpty();
    }
}
