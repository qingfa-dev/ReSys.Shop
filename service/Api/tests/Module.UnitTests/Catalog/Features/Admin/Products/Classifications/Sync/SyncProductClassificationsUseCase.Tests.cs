using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Features.Admin.Products.Classifications.Shared.Models;
using Module.Catalog.Features.Admin.Products.Classifications.Sync;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Classifications.Sync;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductClassificationSync")]
public class SyncProductClassificationsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<SyncProductClassifications.CommandHandler>> _loggerMock;
    private readonly SyncProductClassifications.CommandHandler _handler;

    public SyncProductClassificationsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Classification).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<SyncProductClassifications.CommandHandler>>();

        _handler = new SyncProductClassifications.CommandHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should add new and remove extra classifications")]
    public async Task Handle_ShouldAddAndRemove()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var keepId = Guid.NewGuid();
        var removeId = Guid.NewGuid();
        var addId = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Classification>().Add(ClassificationMethod.Create(product.Id, keepId, 0, false).Value);
        _dbContext.Set<Classification>().Add(ClassificationMethod.Create(product.Id, removeId, 0, false).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncProductClassifications.Request { Items = [new ProductClassificationAssignmentItem { TaxonId = keepId, Position = 0 }, new ProductClassificationAssignmentItem { TaxonId = addId, Position = 1 }] };
        var result = await _handler.Handle(new SyncProductClassifications.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var classifications = await _dbContext.Set<Classification>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        classifications.Should().HaveCount(2);
        classifications.Select(x => x.TaxonId).Should().Contain([keepId, addId]);
        classifications.Select(x => x.TaxonId).Should().NotContain(removeId);
    }

    [Fact(DisplayName = "Handler: Should no-op when already synced")]
    public async Task Handle_ShouldNoOp_WhenAlreadySynced()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var taxonId = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Classification>().Add(ClassificationMethod.Create(product.Id, taxonId, 0, false).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncProductClassifications.Request { Items = [new ProductClassificationAssignmentItem { TaxonId = taxonId, Position = 0 }] };
        var result = await _handler.Handle(new SyncProductClassifications.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var classifications = await _dbContext.Set<Classification>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        classifications.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Handler: Should return failure when product not found")]
    public async Task Handle_ShouldReturnFailure_WhenProductNotFound()
    {
        var request = new SyncProductClassifications.Request { Items = [new ProductClassificationAssignmentItem { TaxonId = Guid.NewGuid(), Position = 0 }] };
        var result = await _handler.Handle(new SyncProductClassifications.Command(Guid.NewGuid(), request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ProductResult.Errors.NotFound(Guid.Empty).Code);
    }

    [Fact(DisplayName = "Handler: Should handle mixed add-remove scenario")]
    public async Task Handle_ShouldHandleMixedScenario()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var keep = Guid.NewGuid();
        var remove = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Classification>().Add(ClassificationMethod.Create(product.Id, keep, 0, false).Value);
        _dbContext.Set<Classification>().Add(ClassificationMethod.Create(product.Id, remove, 0, false).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncProductClassifications.Request { Items = [new ProductClassificationAssignmentItem { TaxonId = keep, Position = 0 }] };
        var result = await _handler.Handle(new SyncProductClassifications.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var allPersisted = await _dbContext.Set<Classification>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        allPersisted.Should().HaveCount(1);
        allPersisted[0].TaxonId.Should().Be(keep);
    }

    [Fact(DisplayName = "Handler: Should remove all when incoming list is empty")]
    public async Task Handle_ShouldRemoveAll_WhenEmptyIncomingList()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var taxonId1 = Guid.NewGuid();
        var taxonId2 = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Classification>().Add(ClassificationMethod.Create(product.Id, taxonId1, 0, false).Value);
        _dbContext.Set<Classification>().Add(ClassificationMethod.Create(product.Id, taxonId2, 0, false).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncProductClassifications.Request { Items = [] };
        var result = await _handler.Handle(new SyncProductClassifications.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var classifications = await _dbContext.Set<Classification>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        classifications.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should add all when existing is empty")]
    public async Task Handle_ShouldAddAll_WhenExistingIsEmpty()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var taxonId1 = Guid.NewGuid();
        var taxonId2 = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncProductClassifications.Request { Items = [new ProductClassificationAssignmentItem { TaxonId = taxonId1, Position = 0 }, new ProductClassificationAssignmentItem { TaxonId = taxonId2, Position = 1 }] };
        var result = await _handler.Handle(new SyncProductClassifications.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var classifications = await _dbContext.Set<Classification>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        classifications.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Handler: Should handle sync with multiple taxons in both sets")]
    public async Task Handle_ShouldHandleLargeSync()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var keep = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToList();
        var remove = Enumerable.Range(0, 3).Select(_ => Guid.NewGuid()).ToList();
        var add = Enumerable.Range(0, 2).Select(_ => Guid.NewGuid()).ToList();

        _dbContext.Set<Product>().Add(product);
        foreach (var id in keep.Concat(remove))
            _dbContext.Set<Classification>().Add(ClassificationMethod.Create(product.Id, id, 0, false).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncProductClassifications.Request { Items = [..keep.Select(x => new ProductClassificationAssignmentItem { TaxonId = x, Position = 0 }), ..add.Select(x => new ProductClassificationAssignmentItem { TaxonId = x, Position = 1 })] };
        var result = await _handler.Handle(new SyncProductClassifications.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var classifications = await _dbContext.Set<Classification>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        classifications.Should().HaveCount(keep.Count + add.Count);
    }

    [Fact(DisplayName = "Handler: Should handle zero existing and empty request as no-op")]
    public async Task Handle_ShouldNoOp_WhenBothEmpty()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncProductClassifications.Request { Items = [] };
        var result = await _handler.Handle(new SyncProductClassifications.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should update position on existing before diff")]
    public async Task Handle_ShouldUpdatePosition_OnExistingBeforeDiff()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var taxonId = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Classification>().Add(ClassificationMethod.Create(product.Id, taxonId, 0, false).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncProductClassifications.Request { Items = [new ProductClassificationAssignmentItem { TaxonId = taxonId, Position = 3 }] };
        var result = await _handler.Handle(new SyncProductClassifications.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var classification = await _dbContext.Set<Classification>().FirstAsync(x => x.ProductId == product.Id, TestContext.Current.CancellationToken);
        classification.Position.Should().Be(3);
    }
}
