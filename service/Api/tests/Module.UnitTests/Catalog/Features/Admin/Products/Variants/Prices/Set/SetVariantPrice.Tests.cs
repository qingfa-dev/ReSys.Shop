using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Prices;
using Module.Catalog.Features.Admin.Products.Variants.Prices.Set;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Prices.Set;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "PriceSet")]
public class SetVariantPriceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<SetVariantPrice.CommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly SetVariantPrice.CommandHandler _handler;

    public SetVariantPriceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Variant).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<SetVariantPrice.CommandHandler>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _handler = new SetVariantPrice.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create new price when none exists for currency")]
    public async Task Handle_ShouldCreatePrice_WhenNoExisting()
    {
        var variant = VariantExtensions.Create(Guid.NewGuid(), "SKU-001", isMaster: true).Value;
        _dbContext.Set<Variant>().Add(variant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SetVariantPrice.Request
        {
            Amount = 29.99m,
            Currency = "USD",
            CountryIso = "US",
        };

        var result = await _handler.Handle(new SetVariantPrice.Command(variant.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<Price>().FirstOrDefaultAsync(p => p.VariantId == variant.Id, cancellationToken: TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.Amount.Should().Be(29.99m);
        persisted.Currency.Should().Be("USD");
    }

    [Fact(DisplayName = "Handler: Should update existing price for same currency")]
    public async Task Handle_ShouldUpdatePrice_WhenExisting()
    {
        var variant = VariantExtensions.Create(Guid.NewGuid(), "SKU-001", isMaster: true).Value;
        _dbContext.Set<Variant>().Add(variant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var existingPrice = PriceExtensions.Create(10m, "USD", variantId: variant.Id, countryIso: "US").Value;
        _dbContext.Set<Price>().Add(existingPrice);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SetVariantPrice.Request
        {
            Amount = 25.00m,
            Currency = "USD",
            CountryIso = "US",
        };

        var result = await _handler.Handle(new SetVariantPrice.Command(variant.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var prices = await _dbContext.Set<Price>().Where(p => p.VariantId == variant.Id).ToListAsync(cancellationToken: TestContext.Current.CancellationToken);
        prices.Should().HaveCount(1);
        prices.First().Amount.Should().Be(25.00m);
    }

    [Fact(DisplayName = "Handler: Should return failure when variant not found")]
    public async Task Handle_ShouldReturnFailure_WhenVariantNotFound()
    {
        var request = new SetVariantPrice.Request { Amount = 10m, Currency = "USD" };

        var result = await _handler.Handle(new SetVariantPrice.Command(Guid.NewGuid(), request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(VariantResult.Errors.NotFound(Guid.Empty).Code);
    }
}
