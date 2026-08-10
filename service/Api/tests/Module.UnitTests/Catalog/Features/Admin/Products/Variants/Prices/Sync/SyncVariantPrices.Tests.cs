using Module.Catalog.Domain.Variants;
using Module.Catalog.Domain.Variants.Prices;
using Module.Catalog.Features.Admin.Variants.Prices.Sync;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Prices.Sync;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "PriceSync")]
public class SyncVariantPricesTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<SyncVariantPrices.CommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly SyncVariantPrices.CommandHandler _handler;

    public SyncVariantPricesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Variant).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<SyncVariantPrices.CommandHandler>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _handler = new SyncVariantPrices.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should add new and remove stale prices")]
    public async Task Handle_ShouldAddAndRemove_WhenDiff()
    {
        var variant = VariantMethod.Create(Guid.NewGuid(), "SKU-001", isMaster: true).Value;
        var stalePrice = PriceMethod.Create(10m, "USD", variantId: variant.Id, countryIso: "US").Value;
        _dbContext.Set<Variant>().Add(variant);
        _dbContext.Set<Price>().Add(stalePrice);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncVariantPrices.Request
        {
            Prices =
            [
                new SyncVariantPrices.SyncPriceItem { Amount = 20m, Currency = "EUR", CountryIso = "GB" },
            ],
        };

        var result = await _handler.Handle(new SyncVariantPrices.Command(variant.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Added.Should().Be(1);
        result.Value.Removed.Should().Be(1);
        result.Value.Updated.Should().Be(0);
    }

    [Fact(DisplayName = "Handler: Should no-op when prices match")]
    public async Task Handle_ShouldNoOp_WhenIdentical()
    {
        var variant = VariantMethod.Create(Guid.NewGuid(), "SKU-001", isMaster: true).Value;
        var price = PriceMethod.Create(10m, "USD", variantId: variant.Id, countryIso: "US").Value;
        _dbContext.Set<Variant>().Add(variant);
        _dbContext.Set<Price>().Add(price);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncVariantPrices.Request
        {
            Prices =
            [
                new SyncVariantPrices.SyncPriceItem { Amount = 10m, Currency = "USD", CountryIso = "US" },
            ],
        };

        var result = await _handler.Handle(new SyncVariantPrices.Command(variant.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Added.Should().Be(0);
        result.Value.Removed.Should().Be(0);
        result.Value.Updated.Should().Be(1);
    }

    [Fact(DisplayName = "Handler: Should remove all when request has empty prices")]
    public async Task Handle_ShouldRemoveAll_WhenEmptyRequest()
    {
        var variant = VariantMethod.Create(Guid.NewGuid(), "SKU-001", isMaster: true).Value;
        var price = PriceMethod.Create(10m, "USD", variantId: variant.Id).Value;
        _dbContext.Set<Variant>().Add(variant);
        _dbContext.Set<Price>().Add(price);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncVariantPrices.Request { Prices = [] };

        var result = await _handler.Handle(new SyncVariantPrices.Command(variant.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Removed.Should().Be(1);

        var remaining = await _dbContext.Set<Price>().IgnoreQueryFilters().CountAsync(p => p.VariantId == variant.Id, cancellationToken: TestContext.Current.CancellationToken);
        remaining.Should().Be(1); // Still in DB but soft-deleted
        var active = await _dbContext.Set<Price>().CountAsync(p => p.VariantId == variant.Id && p.DeletedAt == null, cancellationToken: TestContext.Current.CancellationToken);
        active.Should().Be(0);
    }

    [Fact(DisplayName = "Handler: Should return failure when variant not found")]
    public async Task Handle_ShouldReturnFailure_WhenVariantNotFound()
    {
        var request = new SyncVariantPrices.Request { Prices = [] };

        var result = await _handler.Handle(new SyncVariantPrices.Command(Guid.NewGuid(), request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(VariantResult.Errors.NotFound(Guid.Empty).Code);
    }
}
