using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Options;
using Module.Catalog.Features.Admin.Products.OptionTypes.Revoke;

namespace Module.UnitTests.Catalog.Features.Admin.Products.OptionTypes.Revoke;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductOptionTypeRevoke")]
public class RevokeProductOptionTypesTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<RevokeProductOptionTypes.CommandHandler>> _loggerMock;
    private readonly RevokeProductOptionTypes.CommandHandler _handler;

    public RevokeProductOptionTypesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ProductOptionType).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<RevokeProductOptionTypes.CommandHandler>>();

        _handler = new RevokeProductOptionTypes.CommandHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should remove matching junctions")]
    public async Task Handle_ShouldRemoveMatching()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var ot1 = Guid.NewGuid();
        var ot2 = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(product.Id, ot1).Value);
        _dbContext.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(product.Id, ot2).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new RevokeProductOptionTypes.Request { Items = [new() { OptionTypeId = ot1 }] };
        var result = await _handler.Handle(new RevokeProductOptionTypes.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var junctions = await _dbContext.Set<ProductOptionType>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        junctions.Should().HaveCount(1);
        junctions[0].OptionTypeId.Should().Be(ot2);
    }

    [Fact(DisplayName = "Handler: Should no-op when none match")]
    public async Task Handle_ShouldNoOp_WhenNoneMatch()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;

        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new RevokeProductOptionTypes.Request { Items = [new() { OptionTypeId = Guid.NewGuid() }] };
        var result = await _handler.Handle(new RevokeProductOptionTypes.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when product not found")]
    public async Task Handle_ShouldReturnFailure_WhenProductNotFound()
    {
        var request = new RevokeProductOptionTypes.Request { Items = [new() { OptionTypeId = Guid.NewGuid() }] };
        var result = await _handler.Handle(new RevokeProductOptionTypes.Command(Guid.NewGuid(), request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ProductResult.Errors.NotFound(Guid.Empty).Code);
    }

    [Fact(DisplayName = "Handler: Should remove multiple matching junctions")]
    public async Task Handle_ShouldRemoveMultiple()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var ot1 = Guid.NewGuid();
        var ot2 = Guid.NewGuid();
        var ot3 = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(product.Id, ot1).Value);
        _dbContext.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(product.Id, ot2).Value);
        _dbContext.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(product.Id, ot3).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new RevokeProductOptionTypes.Request { Items = [new() { OptionTypeId = ot1 }, new() { OptionTypeId = ot3 }] };
        var result = await _handler.Handle(new RevokeProductOptionTypes.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var junctions = await _dbContext.Set<ProductOptionType>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        junctions.Should().HaveCount(1);
        junctions[0].OptionTypeId.Should().Be(ot2);
    }

    [Fact(DisplayName = "Handler: Should remove all when all match")]
    public async Task Handle_ShouldRemoveAll_WhenAllMatch()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var ot1 = Guid.NewGuid();
        var ot2 = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(product.Id, ot1).Value);
        _dbContext.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(product.Id, ot2).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new RevokeProductOptionTypes.Request { Items = [new() { OptionTypeId = ot1 }, new() { OptionTypeId = ot2 }] };
        var result = await _handler.Handle(new RevokeProductOptionTypes.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var junctions = await _dbContext.Set<ProductOptionType>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        junctions.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should only remove matching when some exist and some do not")]
    public async Task Handle_ShouldRemovePartial_WhenSomeExistAndSomeDoNot()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var ot1 = Guid.NewGuid();
        var ot2 = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(product.Id, ot1).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new RevokeProductOptionTypes.Request { Items = [new() { OptionTypeId = ot1 }, new() { OptionTypeId = ot2 }] };
        var result = await _handler.Handle(new RevokeProductOptionTypes.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var junctions = await _dbContext.Set<ProductOptionType>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        junctions.Should().BeEmpty();
    }
}
