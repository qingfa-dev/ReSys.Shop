using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Options;
using Module.Catalog.Features.Admin.Products.OptionTypes.Get;

namespace Module.UnitTests.Catalog.Features.Admin.Products.OptionTypes.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductOptionTypeGet")]
public class GetProductOptionTypesTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetProductOptionTypes.QueryHandler _handler;

    public GetProductOptionTypesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ProductOptionType).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetProductOptionTypes.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return items with correct IsAssigned flag and Position")]
    public async Task Handle_ShouldReturnItemsWithCorrectIsAssigned()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var ot1 = OptionTypeMethod.Create("Color", "Color", 0).Value;
        var ot2 = OptionTypeMethod.Create("Size", "Size", 0).Value;

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<OptionType>().AddRange(ot1, ot2);
        _dbContext.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(product.Id, ot1.Id, position: 3).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetProductOptionTypes.Query(product.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.Should().ContainSingle(x => x.OptionTypeId == ot1.Id && x.IsAssigned && x.Position == 3);
        result.Value.Items.Should().ContainSingle(x => x.OptionTypeId == ot2.Id && !x.IsAssigned && x.Position == 0);
    }

    [Fact(DisplayName = "Handler: Should return failure when product not found")]
    public async Task Handle_ShouldReturnFailure_WhenProductNotFound()
    {
        var result = await _handler.Handle(new GetProductOptionTypes.Query(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ProductResult.Errors.NotFound(Guid.Empty).Code);
    }

    [Fact(DisplayName = "Handler: Should return empty items when no option types exist")]
    public async Task Handle_ShouldReturnEmptyItems_WhenNoOptionTypesExist()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetProductOptionTypes.Query(product.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should not bleed assignments from other products")]
    public async Task Handle_ShouldNotBleedFromOtherProducts()
    {
        var productA = ProductMethod.Create("Product A", "product-a").Value;
        var productB = ProductMethod.Create("Product B", "product-b").Value;
        var ot = OptionTypeMethod.Create("Color", "Color", 0).Value;

        _dbContext.Set<Product>().AddRange(productA, productB);
        _dbContext.Set<OptionType>().Add(ot);
        _dbContext.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(productA.Id, ot.Id).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetProductOptionTypes.Query(productB.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle(x => x.OptionTypeId == ot.Id && !x.IsAssigned);
    }
}
