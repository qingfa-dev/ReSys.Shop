using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Options;
using Module.Catalog.Features.Admin.Products.OptionTypes.Shared.Models;
using Module.Catalog.Features.Admin.Products.OptionTypes.Sync;

namespace Module.UnitTests.Catalog.Features.Admin.Products.OptionTypes.Sync;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductOptionTypeSync")]
public class SyncProductOptionTypesTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<SyncProductOptionTypes.CommandHandler>> _loggerMock;
    private readonly SyncProductOptionTypes.CommandHandler _handler;

    public SyncProductOptionTypesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ProductOptionType).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<SyncProductOptionTypes.CommandHandler>>();
        _handler = new SyncProductOptionTypes.CommandHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should add new and remove extra junctions")]
    public async Task Handle_ShouldAddAndRemove()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var keepId = Guid.NewGuid();
        var removeId = Guid.NewGuid();
        var addId = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(product.Id, keepId).Value);
        _dbContext.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(product.Id, removeId).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncProductOptionTypes.Request
        {
            Items = [
                new() { OptionTypeId = keepId, Position = 1 },
                new() { OptionTypeId = addId, Position = 2 }
            ]
        };
        var result = await _handler.Handle(new SyncProductOptionTypes.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var junctions = await _dbContext.Set<ProductOptionType>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        junctions.Should().HaveCount(2);
        junctions.Select(x => x.OptionTypeId).Should().Contain([keepId, addId]);
        junctions.Select(x => x.OptionTypeId).Should().NotContain(removeId);
    }

    [Fact(DisplayName = "Handler: Should no-op when already synced")]
    public async Task Handle_ShouldNoOp_WhenAlreadySynced()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var optionTypeId = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(product.Id, optionTypeId).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncProductOptionTypes.Request { Items = [new() { OptionTypeId = optionTypeId, Position = 1 }] };
        var result = await _handler.Handle(new SyncProductOptionTypes.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var junctions = await _dbContext.Set<ProductOptionType>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        junctions.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Handler: Should return failure when product not found")]
    public async Task Handle_ShouldReturnFailure_WhenProductNotFound()
    {
        var request = new SyncProductOptionTypes.Request { Items = [new() { OptionTypeId = Guid.NewGuid(), Position = 1 }] };
        var result = await _handler.Handle(new SyncProductOptionTypes.Command(Guid.NewGuid(), request), TestContext.Current.CancellationToken);

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
        _dbContext.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(product.Id, keep).Value);
        _dbContext.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(product.Id, remove).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncProductOptionTypes.Request { Items = [new() { OptionTypeId = keep, Position = 1 }] };
        var result = await _handler.Handle(new SyncProductOptionTypes.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var allPersisted = await _dbContext.Set<ProductOptionType>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        allPersisted.Should().HaveCount(1);
        allPersisted[0].OptionTypeId.Should().Be(keep);
    }

    [Fact(DisplayName = "Handler: Should remove all when incoming list is empty")]
    public async Task Handle_ShouldRemoveAll_WhenEmptyIncomingList()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var ot1 = Guid.NewGuid();
        var ot2 = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(product.Id, ot1).Value);
        _dbContext.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(product.Id, ot2).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncProductOptionTypes.Request { Items = [] };
        var result = await _handler.Handle(new SyncProductOptionTypes.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var junctions = await _dbContext.Set<ProductOptionType>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        junctions.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should add all when existing is empty")]
    public async Task Handle_ShouldAddAll_WhenExistingIsEmpty()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var ot1 = Guid.NewGuid();
        var ot2 = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncProductOptionTypes.Request
        {
            Items = [
                new() { OptionTypeId = ot1, Position = 1 },
                new() { OptionTypeId = ot2, Position = 2 }
            ]
        };
        var result = await _handler.Handle(new SyncProductOptionTypes.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var junctions = await _dbContext.Set<ProductOptionType>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        junctions.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Handler: Should handle large sync with multiple IDs")]
    public async Task Handle_ShouldHandleLargeSync()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var keep = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToList();
        var remove = Enumerable.Range(0, 3).Select(_ => Guid.NewGuid()).ToList();
        var add = Enumerable.Range(0, 2).Select(_ => Guid.NewGuid()).ToList();

        _dbContext.Set<Product>().Add(product);
        foreach (var id in keep.Concat(remove))
            _dbContext.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(product.Id, id).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncProductOptionTypes.Request
        {
            Items = keep.Select(id => new ProductOptionTypeAssignmentItem { OptionTypeId = id, Position = 1 })
                .Concat(add.Select(id => new ProductOptionTypeAssignmentItem { OptionTypeId = id, Position = 2 }))
                .ToList()
        };
        var result = await _handler.Handle(new SyncProductOptionTypes.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var junctions = await _dbContext.Set<ProductOptionType>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        junctions.Should().HaveCount(keep.Count + add.Count);
    }

    [Fact(DisplayName = "Handler: Should no-op when both existing and request are empty")]
    public async Task Handle_ShouldNoOp_WhenBothEmpty()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncProductOptionTypes.Request { Items = [] };
        var result = await _handler.Handle(new SyncProductOptionTypes.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }
}
