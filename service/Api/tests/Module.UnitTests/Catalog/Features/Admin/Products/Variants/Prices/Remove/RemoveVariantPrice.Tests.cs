using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Prices;
using Module.Catalog.Features.Admin.Products.Variants.Prices.Remove;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Prices.Remove;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "PriceRemove")]
public class RemoveVariantPriceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<RemoveVariantPrice.CommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly RemoveVariantPrice.CommandHandler _handler;

    public RemoveVariantPriceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Variant).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<RemoveVariantPrice.CommandHandler>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _handler = new RemoveVariantPrice.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should soft-delete price")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var variant = VariantMethod.Create(Guid.NewGuid(), "SKU-001", isMaster: true).Value;
        var price = PriceMethod.Create(10m, "USD", variantId: variant.Id).Value;
        _dbContext.Set<Variant>().Add(variant);
        _dbContext.Set<Price>().Add(price);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new RemoveVariantPrice.Command(variant.Id, price.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var deleted = await _dbContext.Set<Price>().IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == price.Id, cancellationToken: TestContext.Current.CancellationToken);
        deleted.Should().NotBeNull();
        deleted!.DeletedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Handler: Should return failure when variant not found")]
    public async Task Handle_ShouldReturnFailure_WhenVariantNotFound()
    {
        var result = await _handler.Handle(new RemoveVariantPrice.Command(Guid.NewGuid(), Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(VariantResult.Errors.NotFound(Guid.Empty).Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when price not found")]
    public async Task Handle_ShouldReturnFailure_WhenPriceNotFound()
    {
        var variant = VariantMethod.Create(Guid.NewGuid(), "SKU-001", isMaster: true).Value;
        _dbContext.Set<Variant>().Add(variant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new RemoveVariantPrice.Command(variant.Id, Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(PriceResult.Errors.NotFound.Code);
    }
}
