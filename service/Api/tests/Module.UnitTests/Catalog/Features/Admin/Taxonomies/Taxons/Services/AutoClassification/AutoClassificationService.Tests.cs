using System.Globalization;

using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxons.Services.AutoClassification;
using Module.Catalog.Features.Admin.Taxons.Services.AutoClassification.Abstractions;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Services.AutoClassification;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "AutoClassification")]
public class AutoClassificationServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ITaxonRuleEvaluator> _evaluatorMock;
    private readonly AutoClassificationService _service;

    public AutoClassificationServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Taxon).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _evaluatorMock = new Mock<ITaxonRuleEvaluator>();

        _service = new AutoClassificationService(_dbContext, _evaluatorMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "RegenerateForTaxon: Should skip if taxon not found")]
    public async Task RegenerateForTaxon_ShouldSkip_IfNotFound()
    {
        // Act
        await _service.RegenerateForTaxonAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        _evaluatorMock.Verify(x => x.Evaluate(It.IsAny<Product>(), It.IsAny<Taxon>()), Times.Never);
    }

    [Fact(DisplayName = "RegenerateForTaxon: Should skip if taxon is not automatic")]
    public async Task RegenerateForTaxon_ShouldSkip_IfNotAutomatic()
    {
        // Arrange
        var taxon = CreateTaxon("Manual", automatic: false);
        _dbContext.Set<Taxon>().Add(taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        await _service.RegenerateForTaxonAsync(taxon.Id, TestContext.Current.CancellationToken);

        // Assert
        _evaluatorMock.Verify(x => x.Evaluate(It.IsAny<Product>(), It.IsAny<Taxon>()), Times.Never);
    }

    [Fact(DisplayName = "RegenerateForTaxon: Should add new classifications for matching products")]
    public async Task RegenerateForTaxon_ShouldAddClassification_WhenMatches()
    {
        // Arrange
        var taxon = CreateTaxon("Auto", automatic: true);
        var product = CreateProduct("Matching Product");
        
        _dbContext.Set<Taxon>().Add(taxon);
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _evaluatorMock.Setup(x => x.Evaluate(It.IsAny<Product>(), It.IsAny<Taxon>())).Returns(true);

        // Act
        await _service.RegenerateForTaxonAsync(taxon.Id, TestContext.Current.CancellationToken);

        // Assert
        var classifications = await _dbContext.Set<Classification>().ToListAsync(TestContext.Current.CancellationToken);
        classifications.Should().ContainSingle(c => c.ProductId == product.Id && c.TaxonId == taxon.Id && c.IsAutomatic);
        
        var updatedTaxon = await _dbContext.Set<Taxon>().FirstAsync(t => t.Id == taxon.Id, TestContext.Current.CancellationToken);
        updatedTaxon.MarkedForRegenerateTaxonProducts.Should().BeFalse();
    }

    [Fact(DisplayName = "RegenerateForTaxon: Should remove stale automatic classifications")]
    public async Task RegenerateForTaxon_ShouldRemoveClassification_WhenNotMatches()
    {
        // Arrange
        var taxon = CreateTaxon("Auto", automatic: true);
        var product = CreateProduct("Stale Product");
        var stale = ClassificationMethod.Create(product.Id, taxon.Id, isAutomatic: true).Value;
        
        _dbContext.Set<Taxon>().Add(taxon);
        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Classification>().Add(stale);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _evaluatorMock.Setup(x => x.Evaluate(It.IsAny<Product>(), It.IsAny<Taxon>())).Returns(false);

        // Act
        await _service.RegenerateForTaxonAsync(taxon.Id, TestContext.Current.CancellationToken);

        // Assert
        var classifications = await _dbContext.Set<Classification>().ToListAsync(TestContext.Current.CancellationToken);
        classifications.Should().BeEmpty();
    }

    [Fact(DisplayName = "RegenerateForTaxon: Should preserve manual classifications even if rules don't match")]
    public async Task RegenerateForTaxon_ShouldPreserveManualClassifications()
    {
        // Arrange
        var taxon = CreateTaxon("Auto", automatic: true);
        var product = CreateProduct("Manual Match");
        // Manual classification (IsAutomatic = false)
        var manual = ClassificationMethod.Create(product.Id, taxon.Id, isAutomatic: false).Value;
        
        _dbContext.Set<Taxon>().Add(taxon);
        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Classification>().Add(manual);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Evaluator says it doesn't match rules
        _evaluatorMock.Setup(x => x.Evaluate(It.IsAny<Product>(), It.IsAny<Taxon>())).Returns(false);

        // Act
        await _service.RegenerateForTaxonAsync(taxon.Id, TestContext.Current.CancellationToken);

        // Assert
        var classifications = await _dbContext.Set<Classification>().ToListAsync(TestContext.Current.CancellationToken);
        classifications.Should().ContainSingle();
        classifications[0].IsAutomatic.Should().BeFalse("because manual classifications must never be deleted by auto-classification logic");
    }

    [Fact(DisplayName = "RegenerateForTaxon: Should handle multiple batches of products")]
    public async Task RegenerateForTaxon_ShouldHandleMultipleBatches()
    {
        // Arrange
        var taxon = CreateTaxon("Auto", automatic: true);
        _dbContext.Set<Taxon>().Add(taxon);

        // Create 550 products (BatchSize is 500)
        var products = Enumerable.Range(1, 550).Select(i => CreateProduct($"Product {i}")).ToList();
        _dbContext.Set<Product>().AddRange(products);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Setup: All products match
        _evaluatorMock.Setup(x => x.Evaluate(It.IsAny<Product>(), It.IsAny<Taxon>())).Returns(true);

        // Act
        await _service.RegenerateForTaxonAsync(taxon.Id, TestContext.Current.CancellationToken);

        // Assert
        var count = await _dbContext.Set<Classification>().CountAsync(c => c.TaxonId == taxon.Id && c.IsAutomatic, TestContext.Current.CancellationToken);
        count.Should().Be(550, "because the service should have processed both batches (500 + 50)");
        
        _evaluatorMock.Verify(x => x.Evaluate(It.IsAny<Product>(), It.IsAny<Taxon>()), Times.Exactly(550));
    }

    [Fact(DisplayName = "RegenerateForProduct: Should preserve manual classifications on the product")]
    public async Task RegenerateForProduct_ShouldPreserveManualClassifications()
    {
        // Arrange
        var p = CreateProduct("Product");
        var t1 = CreateTaxon("Auto Taxon", automatic: true);
        var t2 = CreateTaxon("Manual Taxon", automatic: false);
        
        // Manual assignment to a non-automatic taxon
        var manual = ClassificationMethod.Create(p.Id, t2.Id, isAutomatic: false).Value;

        _dbContext.Set<Product>().Add(p);
        _dbContext.Set<Taxon>().AddRange(t1, t2);
        _dbContext.Set<Classification>().Add(manual);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _evaluatorMock.Setup(x => x.Evaluate(It.IsAny<Product>(), It.Is<Taxon>(t => t.Id == t1.Id))).Returns(true);

        // Act
        await _service.RegenerateForProductAsync(p.Id, TestContext.Current.CancellationToken);

        // Assert
        var classifications = await _dbContext.Set<Classification>()
            .Where(c => c.ProductId == p.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
            
        classifications.Should().HaveCount(2);
        classifications.Should().Contain(c => c.TaxonId == t1.Id && c.IsAutomatic);
        classifications.Should().Contain(c => c.TaxonId == t2.Id && !c.IsAutomatic, "because the manual classification should not have been touched");
    }

    [Fact(DisplayName = "RegenerateForTaxon: Should not crash if catalog is empty")]
    public async Task RegenerateForTaxon_ShouldHandleEmptyCatalog()
    {
        // Arrange
        var taxon = CreateTaxon("Auto", automatic: true);
        _dbContext.Set<Taxon>().Add(taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act & Assert
        await _service.Invoking(s => s.RegenerateForTaxonAsync(taxon.Id, TestContext.Current.CancellationToken))
            .Should().NotThrowAsync();
            
        var count = await _dbContext.Set<Classification>().CountAsync(TestContext.Current.CancellationToken);
        count.Should().Be(0);
    }

    [Fact(DisplayName = "RegenerateForProduct: Should update all automatic taxons for a product")]
    public async Task RegenerateForProduct_ShouldUpdateClassifications()
    {
        // Arrange
        var p = CreateProduct("Product");
        var t1 = CreateTaxon("T1", automatic: true); // Matches
        var t2 = CreateTaxon("T2", automatic: true); // Does not match
        var t3 = CreateTaxon("T3", automatic: false); // Should be ignored

        _dbContext.Set<Product>().Add(p);
        _dbContext.Set<Taxon>().AddRange(t1, t2, t3);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _evaluatorMock.Setup(x => x.Evaluate(It.IsAny<Product>(), It.Is<Taxon>(t => t.Id == t1.Id))).Returns(true);
        _evaluatorMock.Setup(x => x.Evaluate(It.IsAny<Product>(), It.Is<Taxon>(t => t.Id == t2.Id))).Returns(false);

        p.MarkedForRegenerateTaxonProducts = true;
        _dbContext.Set<Product>().Update(p);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        await _service.RegenerateForProductAsync(p.Id, TestContext.Current.CancellationToken);

        // Assert
        var classifications = await _dbContext.Set<Classification>().ToListAsync(TestContext.Current.CancellationToken);
        classifications.Should().ContainSingle(c => c.TaxonId == t1.Id && c.IsAutomatic);
        classifications.Should().NotContain(c => c.TaxonId == t2.Id);
        classifications.Should().NotContain(c => c.TaxonId == t3.Id);

        var updatedProduct = await _dbContext.Set<Product>().FirstAsync(x => x.Id == p.Id, TestContext.Current.CancellationToken);
        updatedProduct.MarkedForRegenerateTaxonProducts.Should().BeFalse();
    }

    [Fact(DisplayName = "RegenerateDirty: Should process both dirty taxons and products")]
    public async Task RegenerateDirty_ShouldProcessAllDirty()
    {
        // Arrange
        var t = CreateTaxon("Dirty Taxon", automatic: true);
        t.MarkedForRegenerateTaxonProducts = true;
        
        var p = CreateProduct("Dirty Product");
        p.MarkedForRegenerateTaxonProducts = true;

        _dbContext.Set<Taxon>().Add(t);
        _dbContext.Set<Product>().Add(p);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _evaluatorMock.Setup(x => x.Evaluate(It.IsAny<Product>(), It.IsAny<Taxon>())).Returns(true);

        // Act
        await _service.RegenerateDirtyAsync(TestContext.Current.CancellationToken);

        // Assert
        var updatedTaxon = await _dbContext.Set<Taxon>().FirstAsync(x => x.Id == t.Id, TestContext.Current.CancellationToken);
        var updatedProduct = await _dbContext.Set<Product>().FirstAsync(x => x.Id == p.Id, TestContext.Current.CancellationToken);

        updatedTaxon.MarkedForRegenerateTaxonProducts.Should().BeFalse();
        updatedProduct.MarkedForRegenerateTaxonProducts.Should().BeFalse();
        
        var classifications = await _dbContext.Set<Classification>().ToListAsync(TestContext.Current.CancellationToken);
        classifications.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "RegenerateDirty: Should skip when nothing is dirty")]
    public async Task RegenerateDirty_ShouldSkip_WhenNothingDirty()
    {
        _evaluatorMock.Setup(x => x.Evaluate(It.IsAny<Product>(), It.IsAny<Taxon>())).Returns(true);

        await _service.RegenerateDirtyAsync(TestContext.Current.CancellationToken);

        _evaluatorMock.Verify(x => x.Evaluate(It.IsAny<Product>(), It.IsAny<Taxon>()), Times.Never);
    }

    [Fact(DisplayName = "RegenerateForProduct: Should skip when product not found")]
    public async Task RegenerateForProduct_ShouldSkip_WhenProductNotFound()
    {
        await _service.RegenerateForProductAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        _evaluatorMock.Verify(x => x.Evaluate(It.IsAny<Product>(), It.IsAny<Taxon>()), Times.Never);
    }

    private static Product CreateProduct(string name)
    {
        return ProductMethod.Create(name, name.ToLower(CultureInfo.InvariantCulture).Replace(" ", "-")).Value;
    }

    private static Taxon CreateTaxon(string name, bool automatic)
    {
        return TaxonMethod.Create(
            taxonomyId: Guid.NewGuid(),
            parentId: null,
            name: name,
            presentation: name,
            description: name,
            position: 0,
            slug: name.ToLower(CultureInfo.InvariantCulture).Replace(" ", "-"),
            metaTitle: null,
            metaDescription: null,
            metaKeywords: null,
            automatic: automatic,
            rulesMatchPolicy: TaxonMatchPolicy.All,
            sortOrder: TaxonSortOrder.Manual,
            hideFromNav: false,
            imageUrl: null,
            squareImageUrl: null).Value;
    }
}
