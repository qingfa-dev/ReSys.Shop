using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Dashboard.Get;

namespace Module.UnitTests.Catalog.Features.Admin.Dashboard.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "GetCatalogDashboard")]
public class GetCatalogDashboardHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetCatalogDashboard.QueryHandler _handler;

    public GetCatalogDashboardHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetCatalogDashboard.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should return all zeros when database is empty")]
    public async Task Handle_ShouldReturnEmpty_WhenDatabaseIsEmpty()
    {
        var ct = TestContext.Current.CancellationToken;

        var result = await _handler.Handle(new GetCatalogDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.TotalProducts.Should().Be(0);
        response.ActiveProducts.Should().Be(0);
        response.DraftProducts.Should().Be(0);
        response.TotalVariants.Should().Be(0);
        response.TotalTaxonomies.Should().Be(0);
        response.TotalTaxons.Should().Be(0);
        response.RecentProducts.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handle: Should count products by status correctly")]
    public async Task Handle_ShouldCountProductsByStatus()
    {
        var ct = TestContext.Current.CancellationToken;

        _dbContext.Set<Product>().Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Active",
            Slug = "active",
            Status = ProductStatus.Active,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = false
        });
        _dbContext.Set<Product>().Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Draft",
            Slug = "draft",
            Status = ProductStatus.Draft,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = false
        });
        _dbContext.Set<Product>().Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Archived",
            Slug = "archived",
            Status = ProductStatus.Archived,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetCatalogDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.TotalProducts.Should().Be(3);
        response.ActiveProducts.Should().Be(1);
        response.DraftProducts.Should().Be(1);
    }

    [Fact(DisplayName = "Handle: Should exclude soft-deleted products and variants")]
    public async Task Handle_ShouldExcludeDeletedEntities()
    {
        var ct = TestContext.Current.CancellationToken;

        _dbContext.Set<Product>().Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Visible",
            Slug = "visible",
            Status = ProductStatus.Active,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = false
        });
        _dbContext.Set<Product>().Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Deleted",
            Slug = "deleted",
            Status = ProductStatus.Active,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = true
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetCatalogDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalProducts.Should().Be(1);
        result.Value.ActiveProducts.Should().Be(1);
    }

    [Fact(DisplayName = "Handle: Should return top 5 recent products ordered by CreatedAtUtc")]
    public async Task Handle_ShouldReturnRecentProducts()
    {
        var ct = TestContext.Current.CancellationToken;
        var baseTime = DateTimeOffset.UtcNow;

        for (int i = 1; i <= 7; i++)
        {
            _dbContext.Set<Product>().Add(new Product
            {
                Id = Guid.NewGuid(),
                Name = $"Product {i}",
                Slug = $"product-{i}",
                Status = ProductStatus.Active,
                CreatedAtUtc = baseTime.AddDays(-i),
                IsDeleted = false
            });
        }
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetCatalogDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var recent = result.Value.RecentProducts;
        recent.Should().HaveCount(5);
        recent.Should().BeInDescendingOrder(p => p.CreatedAtUtc);
    }

    [Fact(DisplayName = "Handle: Should count variants and taxonomies")]
    public async Task Handle_ShouldCountVariantsAndTaxonomies()
    {
        var ct = TestContext.Current.CancellationToken;

        _dbContext.Set<Variant>().Add(new Variant
        {
            Id = Guid.NewGuid(),
            Sku = "MASTER-SKU",
            IsMaster = true,
            IsDeleted = false
        });
        _dbContext.Set<Variant>().Add(new Variant
        {
            Id = Guid.NewGuid(),
            Sku = "VARIANT-SKU",
            IsMaster = false,
            IsDeleted = false
        });
        _dbContext.Set<Taxonomy>().Add(new Taxonomy
        {
            Id = Guid.NewGuid(),
            Name = "Categories",
            IsDeleted = false
        });
        _dbContext.Set<Taxon>().Add(new Taxon
        {
            Id = Guid.NewGuid(),
            Name = "Shoes",
            TaxonomyId = Guid.NewGuid(),
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetCatalogDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalVariants.Should().Be(2);
        result.Value.TotalTaxonomies.Should().Be(1);
        result.Value.TotalTaxons.Should().Be(1);
    }
}
