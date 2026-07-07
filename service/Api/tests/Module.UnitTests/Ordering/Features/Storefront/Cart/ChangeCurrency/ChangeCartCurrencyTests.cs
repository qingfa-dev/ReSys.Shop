using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.ChangeCurrency;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.ChangeCurrency;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "ChangeCartCurrency")]
public class ChangeCartCurrencyTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ChangeCartCurrency.CommandHandler _handler;

    public ChangeCartCurrencyTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new ChangeCartCurrency.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should change cart currency")]
    public async Task Handle_ShouldChangeCurrency()
    {
        var order = OrderExtensions.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty).Value;
        var lineItem = LineItemExtensions.Create(order.Id, Guid.NewGuid(), 1, 10m).Value;
        lineItem.Currency = "USD";
        order.LineItems.Add(lineItem);
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new ChangeCartCurrency.Command(order.Id, new ChangeCartCurrency.Request { Currency = "EUR" }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Currency.Should().Be("EUR");

        var persisted = await _dbContext.Set<Order>()
            .Include(o => o.LineItems)
            .FirstAsync(o => o.Id == order.Id, TestContext.Current.CancellationToken);
        persisted.Currency.Should().Be("EUR");
        persisted.LineItems.All(li => li.Currency == "EUR").Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return not found when cart missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(
            new ChangeCartCurrency.Command(Guid.NewGuid(), new ChangeCartCurrency.Request { Currency = "EUR" }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be(OrderResult.Errors.NotFound(Guid.NewGuid()).Code);
    }
}
