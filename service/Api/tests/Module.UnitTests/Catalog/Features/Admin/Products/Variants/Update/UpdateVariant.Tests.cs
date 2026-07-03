using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Features.Admin.Products.Variants.Update;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "VariantUpdate")]
public class UpdateVariantTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<UpdateVariant.CommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly UpdateVariant.CommandHandler _handler;

    public UpdateVariantTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Variant).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<UpdateVariant.CommandHandler>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _handler = new UpdateVariant.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should update variant fields")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var variant = VariantExtensions.Create(Guid.NewGuid(), "OLD-SKU", isMaster: true, position: 0).Value;
        variant.Price = 10m;
        _dbContext.Set<Variant>().Add(variant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateVariant.Request
        {
            Sku = "NEW-SKU",
            Position = 5,
            TrackInventory = false,
            Price = 29.99m,
            CostPrice = 15m,
            CostCurrency = "USD",
            Weight = 1.5m,
            WeightUnit = "kg",
            Height = 10m,
            Width = 20m,
            Depth = 5m,
            DimensionsUnit = "cm",
        };

        var result = await _handler.Handle(new UpdateVariant.Command(variant.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<Variant>().FirstOrDefaultAsync(x => x.Id == variant.Id, cancellationToken: TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.Sku.Should().Be("NEW-SKU");
        persisted.Position.Should().Be(5);
        persisted.TrackInventory.Should().BeFalse();
        persisted.Price.Should().Be(29.99m);
        persisted.CostPrice.Should().Be(15m);
        persisted.Weight.Should().Be(1.5m);
    }

    [Fact(DisplayName = "Handler: Should return failure when variant not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        var request = new UpdateVariant.Request { Sku = "SKU" };

        var result = await _handler.Handle(new UpdateVariant.Command(Guid.NewGuid(), request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(VariantResult.Errors.NotFound(Guid.Empty).Code);
    }

    [Fact(DisplayName = "Handler: Partial update should preserve other fields")]
    public async Task Handle_ShouldPreserveOtherFields_WhenPartialUpdate()
    {
        var variant = VariantExtensions.Create(Guid.NewGuid(), "SKU-001", isMaster: true, position: 0).Value;
        variant.Price = 10m;
        variant.CostPrice = 5m;
        variant.CostCurrency = "USD";
        _dbContext.Set<Variant>().Add(variant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateVariant.Request
        {
            Sku = "SKU-002",
            TrackInventory = variant.TrackInventory,
            Price = variant.Price,
            CostPrice = variant.CostPrice,
            CostCurrency = variant.CostCurrency,
        };

        var result = await _handler.Handle(new UpdateVariant.Command(variant.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var persisted = await _dbContext.Set<Variant>().FirstOrDefaultAsync(x => x.Id == variant.Id, cancellationToken: TestContext.Current.CancellationToken);
        persisted!.Sku.Should().Be("SKU-002");
        persisted.Price.Should().Be(10m);
        persisted.CostPrice.Should().Be(5m);
    }
}
