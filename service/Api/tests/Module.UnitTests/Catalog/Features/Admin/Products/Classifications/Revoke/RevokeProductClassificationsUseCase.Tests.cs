using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Features.Admin.Products.ProductClassifications.Revoke;
using Module.Catalog.Features.Admin.Products.ProductClassifications.Shared.Models;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Classifications.Revoke;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductClassificationRevoke")]
public class RevokeProductClassificationsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<RevokeProductClassifications.CommandHandler>> _loggerMock;
    private readonly RevokeProductClassifications.CommandHandler _handler;

    public RevokeProductClassificationsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Classification).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<RevokeProductClassifications.CommandHandler>>();

        _handler = new RevokeProductClassifications.CommandHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should remove matching classifications")]
    public async Task Handle_ShouldRemoveMatchingClassifications()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var taxonId1 = Guid.NewGuid();
        var taxonId2 = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Classification>().Add(ClassificationMethod.Create(product.Id, taxonId1, 0, false).Value);
        _dbContext.Set<Classification>().Add(ClassificationMethod.Create(product.Id, taxonId2, 0, false).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new RevokeProductClassifications.Request { Items = [new ProductClassificationAssignmentItem { TaxonId = taxonId1, Position = 0 }] };
        var result = await _handler.Handle(new RevokeProductClassifications.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var classifications = await _dbContext.Set<Classification>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        classifications.Should().HaveCount(1);
        classifications[0].TaxonId.Should().Be(taxonId2);
    }

    [Fact(DisplayName = "Handler: Should no-op when none match")]
    public async Task Handle_ShouldNoOp_WhenNoneMatch()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;

        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new RevokeProductClassifications.Request { Items = [new ProductClassificationAssignmentItem { TaxonId = Guid.NewGuid(), Position = 0 }] };
        var result = await _handler.Handle(new RevokeProductClassifications.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when product not found")]
    public async Task Handle_ShouldReturnFailure_WhenProductNotFound()
    {
        var request = new RevokeProductClassifications.Request { Items = [new ProductClassificationAssignmentItem { TaxonId = Guid.NewGuid(), Position = 0 }] };
        var result = await _handler.Handle(new RevokeProductClassifications.Command(Guid.NewGuid(), request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ProductResult.Errors.NotFound(Guid.Empty).Code);
    }

    [Fact(DisplayName = "Handler: Should remove multiple matching classifications")]
    public async Task Handle_ShouldRemoveMultipleMatchingClassifications()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var taxonId1 = Guid.NewGuid();
        var taxonId2 = Guid.NewGuid();
        var taxonId3 = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Classification>().Add(ClassificationMethod.Create(product.Id, taxonId1, 0, false).Value);
        _dbContext.Set<Classification>().Add(ClassificationMethod.Create(product.Id, taxonId2, 0, false).Value);
        _dbContext.Set<Classification>().Add(ClassificationMethod.Create(product.Id, taxonId3, 0, false).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new RevokeProductClassifications.Request { Items = [new ProductClassificationAssignmentItem { TaxonId = taxonId1, Position = 0 }, new ProductClassificationAssignmentItem { TaxonId = taxonId3, Position = 0 }] };
        var result = await _handler.Handle(new RevokeProductClassifications.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var classifications = await _dbContext.Set<Classification>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        classifications.Should().HaveCount(1);
        classifications[0].TaxonId.Should().Be(taxonId2);
    }

    [Fact(DisplayName = "Handler: Should remove all classifications when all taxon IDs match")]
    public async Task Handle_ShouldRemoveAll_WhenAllMatch()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var taxonId1 = Guid.NewGuid();
        var taxonId2 = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Classification>().Add(ClassificationMethod.Create(product.Id, taxonId1, 0, false).Value);
        _dbContext.Set<Classification>().Add(ClassificationMethod.Create(product.Id, taxonId2, 0, false).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new RevokeProductClassifications.Request { Items = [new ProductClassificationAssignmentItem { TaxonId = taxonId1, Position = 0 }, new ProductClassificationAssignmentItem { TaxonId = taxonId2, Position = 0 }] };
        var result = await _handler.Handle(new RevokeProductClassifications.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var classifications = await _dbContext.Set<Classification>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        classifications.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should only remove matching taxons when some exist and some do not")]
    public async Task Handle_ShouldRemovePartial_WhenSomeExistAndSomeDoNot()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var taxonId1 = Guid.NewGuid();
        var taxonId2 = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Classification>().Add(ClassificationMethod.Create(product.Id, taxonId1, 0, false).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new RevokeProductClassifications.Request { Items = [new ProductClassificationAssignmentItem { TaxonId = taxonId1, Position = 0 }, new ProductClassificationAssignmentItem { TaxonId = taxonId2, Position = 0 }] };
        var result = await _handler.Handle(new RevokeProductClassifications.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var classifications = await _dbContext.Set<Classification>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        classifications.Should().BeEmpty();
    }
}
