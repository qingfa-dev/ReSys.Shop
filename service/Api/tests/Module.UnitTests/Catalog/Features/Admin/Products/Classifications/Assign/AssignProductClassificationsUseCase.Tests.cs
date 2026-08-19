using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Features.Admin.Products.ProductClassifications.Assign;
using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Classifications.Assign;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductClassificationAssign")]
public class AssignProductClassificationsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<AssignProductClassifications.CommandHandler>> _loggerMock;
    private readonly AssignProductClassifications.CommandHandler _handler;

    public AssignProductClassificationsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Classification).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<AssignProductClassifications.CommandHandler>>();

        _handler = new AssignProductClassifications.CommandHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should add new classifications")]
    public async Task Handle_ShouldAddNewClassifications()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var taxonId = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new AssignProductClassifications.Request { Items = [new ProductClassificationAssignmentItem { TaxonId = taxonId, Position = 1 }] };
        var result = await _handler.Handle(new AssignProductClassifications.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var classifications = await _dbContext.Set<Classification>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        classifications.Should().HaveCount(1);
        classifications[0].TaxonId.Should().Be(taxonId);
        classifications[0].Position.Should().Be(1);
    }

    [Fact(DisplayName = "Handler: Should skip duplicates")]
    public async Task Handle_ShouldSkipDuplicates()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var taxonId = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Classification>().Add(ClassificationMethod.Create(product.Id, taxonId, 0, false).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new AssignProductClassifications.Request { Items = [new ProductClassificationAssignmentItem { TaxonId = taxonId, Position = 0 }, new ProductClassificationAssignmentItem { TaxonId = Guid.NewGuid(), Position = 0 }] };
        var result = await _handler.Handle(new AssignProductClassifications.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var classifications = await _dbContext.Set<Classification>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        classifications.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Handler: Should return failure when product not found")]
    public async Task Handle_ShouldReturnFailure_WhenProductNotFound()
    {
        var request = new AssignProductClassifications.Request { Items = [new ProductClassificationAssignmentItem { TaxonId = Guid.NewGuid(), Position = 0 }] };
        var result = await _handler.Handle(new AssignProductClassifications.Command(Guid.NewGuid(), request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ProductResult.Errors.NotFound(Guid.Empty).Code);
    }

    [Fact(DisplayName = "Handler: Should assign multiple taxon IDs in a single call")]
    public async Task Handle_ShouldAssignMultipleTaxonIds()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var taxonId1 = Guid.NewGuid();
        var taxonId2 = Guid.NewGuid();
        var taxonId3 = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new AssignProductClassifications.Request { Items = [new ProductClassificationAssignmentItem { TaxonId = taxonId1, Position = 0 }, new ProductClassificationAssignmentItem { TaxonId = taxonId2, Position = 1 }, new ProductClassificationAssignmentItem { TaxonId = taxonId3, Position = 2 }] };
        var result = await _handler.Handle(new AssignProductClassifications.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var classifications = await _dbContext.Set<Classification>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        classifications.Should().HaveCount(3);
    }

    [Fact(DisplayName = "Handler: Should return Ok when no taxon IDs are new (all duplicates)")]
    public async Task Handle_ShouldReturnOk_WhenAllDuplicates()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var taxonId = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Classification>().Add(ClassificationMethod.Create(product.Id, taxonId, 0, false).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new AssignProductClassifications.Request { Items = [new ProductClassificationAssignmentItem { TaxonId = taxonId, Position = 0 }] };
        var result = await _handler.Handle(new AssignProductClassifications.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var classifications = await _dbContext.Set<Classification>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        classifications.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Handler: Should update position on existing assignments")]
    public async Task Handle_ShouldUpdatePosition_OnExistingAssignments()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var taxonId = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Classification>().Add(ClassificationMethod.Create(product.Id, taxonId, 0, false).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new AssignProductClassifications.Request { Items = [new ProductClassificationAssignmentItem { TaxonId = taxonId, Position = 5 }] };
        var result = await _handler.Handle(new AssignProductClassifications.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var classification = await _dbContext.Set<Classification>().FirstAsync(x => x.ProductId == product.Id, TestContext.Current.CancellationToken);
        classification.Position.Should().Be(5);
    }
}
