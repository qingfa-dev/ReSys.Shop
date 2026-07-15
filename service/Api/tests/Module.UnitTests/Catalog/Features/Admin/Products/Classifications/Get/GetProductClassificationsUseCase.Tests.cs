using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Products.Classifications.Get;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Classifications.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductClassificationGet")]
public class GetProductClassificationsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetProductClassifications.QueryHandler _handler;

    public GetProductClassificationsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Taxon).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetProductClassifications.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return items with correct IsAssigned flag")]
    public async Task Handle_ShouldReturnItemsWithCorrectIsAssigned()
    {
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var taxon1 = TaxonMethod.Create(taxonomy.Id, null, "Shirts", "Shirts", null, 0, "shirts", null, null, null, false, null, null, false, null, null).Value;
        var taxon2 = TaxonMethod.Create(taxonomy.Id, null, "Pants", "Pants", null, 0, "pants", null, null, null, false, null, null, false, null, null).Value;
        var product = ProductMethod.Create("Test Product", "test-product").Value;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(taxon1, taxon2);
        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Classification>().Add(ClassificationMethod.Create(product.Id, taxon1.Id, 0, false).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetProductClassifications.Query(product.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.Should().ContainSingle(x => x.TaxonId == taxon1.Id && x.IsAssigned && x.Position == 0);
        result.Value.Items.Should().ContainSingle(x => x.TaxonId == taxon2.Id && !x.IsAssigned && x.Position == 0);
    }

    [Fact(DisplayName = "Handler: Should return failure when product not found")]
    public async Task Handle_ShouldReturnFailure_WhenProductNotFound()
    {
        var result = await _handler.Handle(new GetProductClassifications.Query(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ProductResult.Errors.NotFound(Guid.Empty).Code);
    }

    [Fact(DisplayName = "Handler: Should return empty items when no taxons exist")]
    public async Task Handle_ShouldReturnEmptyItems_WhenNoTaxonsExist()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetProductClassifications.Query(product.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should include taxons from multiple taxonomies")]
    public async Task Handle_ShouldIncludeTaxonsFromMultipleTaxonomies()
    {
        var taxonomy1 = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var taxonomy2 = TaxonomyMethod.Create("Brands", "Brands", 0).Value;
        var taxon1 = TaxonMethod.Create(taxonomy1.Id, null, "Shirts", "Shirts", null, 0, "shirts", null, null, null, false, null, null, false, null, null).Value;
        var taxon2 = TaxonMethod.Create(taxonomy2.Id, null, "Nike", "Nike", null, 0, "nike", null, null, null, false, null, null, false, null, null).Value;
        var product = ProductMethod.Create("Test Product", "test-product").Value;

        _dbContext.Set<Taxonomy>().AddRange(taxonomy1, taxonomy2);
        _dbContext.Set<Taxon>().AddRange(taxon1, taxon2);
        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Classification>().Add(ClassificationMethod.Create(product.Id, taxon1.Id, 0, false).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetProductClassifications.Query(product.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Handler: Should not bleed classifications from other products")]
    public async Task Handle_ShouldNotBleedClassificationsFromOtherProducts()
    {
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, null, "Shirts", "Shirts", null, 0, "shirts", null, null, null, false, null, null, false, null, null).Value;
        var productA = ProductMethod.Create("Product A", "product-a").Value;
        var productB = ProductMethod.Create("Product B", "product-b").Value;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(taxon);
        _dbContext.Set<Product>().AddRange(productA, productB);
        _dbContext.Set<Classification>().Add(ClassificationMethod.Create(productA.Id, taxon.Id, 0, false).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetProductClassifications.Query(productB.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle(x => x.TaxonId == taxon.Id && !x.IsAssigned);
    }

    [Fact(DisplayName = "Handler: Should exclude soft-deleted taxons")]
    public async Task Handle_ShouldExcludeSoftDeletedTaxons()
    {
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var activeTaxon = TaxonMethod.Create(taxonomy.Id, null, "Active", "Active", null, 0, "active", null, null, null, false, null, null, false, null, null).Value;
        var deletedTaxon = TaxonMethod.Create(taxonomy.Id, null, "Deleted", "Deleted", null, 0, "deleted", null, null, null, false, null, null, false, null, null).Value;
        deletedTaxon.Delete();
        var product = ProductMethod.Create("Test Product", "test-product").Value;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(activeTaxon, deletedTaxon);
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetProductClassifications.Query(product.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle(x => x.TaxonId == activeTaxon.Id);
        result.Value.Items.Should().NotContain(x => x.TaxonId == deletedTaxon.Id);
    }
}
