using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Features.Admin.Products.Variants.Get.PagedOrAll;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.List;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "VariantList")]
public class ListVariantsByProductTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetVariantsPagedOrAll.PagedQueryHandler _handler;

    public ListVariantsByProductTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Variant).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetVariantsPagedOrAll.PagedQueryHandler(_dbContext);
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
        var variant1 = VariantMethod.Create(productId, "SKU-001", isMaster: true).Value;
        var variant2 = VariantMethod.Create(productId, "SKU-002", isMaster: false).Value;
        _dbContext.Set<Variant>().AddRange(variant1, variant2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetVariantsPagedOrAll.Query(new GetVariantsPagedOrAll.Parameters()), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Handler: Should exclude soft-deleted variants")]
    public async Task Handle_ShouldExcludeSoftDeletedVariants()
    {
        var productId = Guid.NewGuid();
        var active = VariantMethod.Create(productId, "SKU-001", isMaster: true).Value;
        var deleted = VariantMethod.Create(productId, "SKU-002", isMaster: false).Value;
        deleted.Delete("admin");
        _dbContext.Set<Variant>().AddRange(active, deleted);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetVariantsPagedOrAll.Query(new GetVariantsPagedOrAll.Parameters()), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
        result.Items.First().Sku.Should().Be("SKU-001");
    }

    [Fact(DisplayName = "Handler: Should return empty when product has no variants")]
    public async Task Handle_ShouldReturnEmpty_WhenNoVariants()
    {
        var result = await _handler.Handle(new GetVariantsPagedOrAll.Query(new GetVariantsPagedOrAll.Parameters()), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should page variants when parameters supplied")]
    public async Task Handle_ShouldPage_WhenParametersSupplied()
    {
        var productId = Guid.NewGuid();
        var variant1 = VariantMethod.Create(productId, "SKU-001", isMaster: true).Value;
        var variant2 = VariantMethod.Create(productId, "SKU-002", isMaster: false).Value;
        var variant3 = VariantMethod.Create(productId, "SKU-003", isMaster: false).Value;
        _dbContext.Set<Variant>().AddRange(variant1, variant2, variant3);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetVariantsPagedOrAll.Query(new GetVariantsPagedOrAll.Parameters { PageSize = 2, ProductId = productId }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
    }

    [Fact(DisplayName = "Handler: Should sort variants by position descending when specified")]
    public async Task Handle_ShouldSortByPositionDescending_WhenSpecified()
    {
        var productId = Guid.NewGuid();
        var variant1 = VariantMethod.Create(productId, "SKU-001", isMaster: true, position: 1).Value;
        var variant2 = VariantMethod.Create(productId, "SKU-002", isMaster: false, position: 2).Value;
        var variant3 = VariantMethod.Create(productId, "SKU-003", isMaster: false, position: 3).Value;
        _dbContext.Set<Variant>().AddRange(variant1, variant2, variant3);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetVariantsPagedOrAll.Query(new GetVariantsPagedOrAll.Parameters { Sort = ["Position:desc"], ProductId = productId }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(3);
        result.Items.Should().BeInDescendingOrder(i => i.Position);
    }

    [Fact(DisplayName = "Handler: Should silently ignore disallowed sort field")]
    public async Task Handle_ShouldIgnoreDisallowedSortField()
    {
        var productId = Guid.NewGuid();
        var variant = VariantMethod.Create(productId, "SKU-001", isMaster: true).Value;
        _dbContext.Set<Variant>().Add(variant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetVariantsPagedOrAll.Query(new GetVariantsPagedOrAll.Parameters { Sort = ["NonExistent:asc"], ProductId = productId }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Handler: Should silently ignore disallowed filter field")]
    public async Task Handle_ShouldIgnoreDisallowedFilterField()
    {
        var productId = Guid.NewGuid();
        var variant1 = VariantMethod.Create(productId, "SKU-001", isMaster: true).Value;
        var variant2 = VariantMethod.Create(productId, "SKU-002", isMaster: false).Value;
        _dbContext.Set<Variant>().AddRange(variant1, variant2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetVariantsPagedOrAll.Query(new GetVariantsPagedOrAll.Parameters { Filter = "NonExistent=1", ProductId = productId }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
    }
}
