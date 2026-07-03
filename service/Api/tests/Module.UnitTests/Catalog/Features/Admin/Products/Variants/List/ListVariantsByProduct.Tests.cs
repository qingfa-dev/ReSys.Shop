using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Features.Admin.Products.Variants.List;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.List;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "VariantList")]
public class ListVariantsByProductTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ListVariantsByProduct.QueryHandler _handler;

    public ListVariantsByProductTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Variant).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new ListVariantsByProduct.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return all non-deleted variants for product")]
    public async Task Handle_ShouldReturnVariants_WhenProductHasVariants()
    {
        var productId = Guid.NewGuid();
        var variant1 = VariantExtensions.Create(productId, "SKU-001", isMaster: true).Value;
        var variant2 = VariantExtensions.Create(productId, "SKU-002", isMaster: false).Value;
        _dbContext.Set<Variant>().AddRange(variant1, variant2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new ListVariantsByProduct.Query(productId), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Handler: Should exclude soft-deleted variants")]
    public async Task Handle_ShouldExcludeSoftDeletedVariants()
    {
        var productId = Guid.NewGuid();
        var active = VariantExtensions.Create(productId, "SKU-001", isMaster: true).Value;
        var deleted = VariantExtensions.Create(productId, "SKU-002", isMaster: false).Value;
        deleted.Delete("admin");
        _dbContext.Set<Variant>().AddRange(active, deleted);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new ListVariantsByProduct.Query(productId), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().Sku.Should().Be("SKU-001");
    }

    [Fact(DisplayName = "Handler: Should return empty when product has no variants")]
    public async Task Handle_ShouldReturnEmpty_WhenNoVariants()
    {
        var result = await _handler.Handle(new ListVariantsByProduct.Query(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }
}
