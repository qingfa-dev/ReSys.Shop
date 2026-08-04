using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Options;
using Module.Catalog.Features.Admin.Products.Options.Assign;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Options.Assign;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductOptionTypeAssign")]
public class AssignProductOptionTypesTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<AssignProductOptionTypes.CommandHandler>> _loggerMock;
    private readonly AssignProductOptionTypes.CommandHandler _handler;

    public AssignProductOptionTypesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ProductOptionType).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<AssignProductOptionTypes.CommandHandler>>();

        _handler = new AssignProductOptionTypes.CommandHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should add new junctions")]
    public async Task Handle_ShouldAddNewJunctions()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var optionTypeId = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new AssignProductOptionTypes.Request { ProductId = product.Id, Items = [new() { OptionTypeId = optionTypeId, Position = 1 }] };
        var result = await _handler.Handle(new AssignProductOptionTypes.Command(request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var junctions = await _dbContext.Set<ProductOptionType>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        junctions.Should().HaveCount(1);
        junctions[0].OptionTypeId.Should().Be(optionTypeId);
        junctions[0].Position.Should().Be(1);
    }

    [Fact(DisplayName = "Handler: Should update position on existing junctions")]
    public async Task Handle_ShouldUpdatePositionOnExisting()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;
        var optionTypeId = Guid.NewGuid();

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(product.Id, optionTypeId, position: 1).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new AssignProductOptionTypes.Request
        {
            ProductId = product.Id,
            Items = [new() { OptionTypeId = optionTypeId, Position = 5, }]
        };
        var result = await _handler.Handle(new AssignProductOptionTypes.Command(request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var junction = await _dbContext.Set<ProductOptionType>().SingleAsync(x => x.OptionTypeId == optionTypeId, TestContext.Current.CancellationToken);
        junction.Position.Should().Be(5);
    }

    [Fact(DisplayName = "Handler: Should return failure when product not found")]
    public async Task Handle_ShouldReturnFailure_WhenProductNotFound()
    {
        var request = new AssignProductOptionTypes.Request { Items = [new() { OptionTypeId = Guid.NewGuid(), Position = 1 }] };
        var result = await _handler.Handle(new AssignProductOptionTypes.Command(request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ProductResult.Errors.NotFound(Guid.Empty).Code);
    }

    [Fact(DisplayName = "Handler: Should assign multiple with different positions")]
    public async Task Handle_ShouldAssignMultipleWithDifferentPositions()
    {
        var product = ProductMethod.Create("Test Product", "test-product").Value;

        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new AssignProductOptionTypes.Request
        {
            ProductId = product.Id,
            Items = [
                new() { OptionTypeId = Guid.NewGuid(), Position = 1 },
                new() { OptionTypeId = Guid.NewGuid(), Position = 2 },
                new() { OptionTypeId = Guid.NewGuid(), Position = 3 }
            ]
        };
        var result = await _handler.Handle(new AssignProductOptionTypes.Command(request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var junctions = await _dbContext.Set<ProductOptionType>().Where(x => x.ProductId == product.Id).ToListAsync(TestContext.Current.CancellationToken);
        junctions.Should().HaveCount(3);
    }
}
