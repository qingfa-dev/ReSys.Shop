using Module.Ordering.Domain.Orders;
using OrderingAdvanceCheckout = Module.Ordering.Features.Storefront.Cart.AdvanceCheckout;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.AdvanceCheckout;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "AdvanceCheckout")]
public class AdvanceCheckoutTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly OrderingAdvanceCheckout.AdvanceCheckout.CommandHandler _handler;
    private readonly Guid _userId;

    public AdvanceCheckoutTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _userId = Guid.NewGuid();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserId).Returns(_userId.ToString());

        _handler = new OrderingAdvanceCheckout.AdvanceCheckout.CommandHandler(_dbContext, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should advance from Address to Delivery when addresses set")]
    public async Task Handle_ShouldAdvance_WhenAddressesSet()
    {
        var cart = OrderExtensions.Create("USD", _userId, Guid.Empty).Value;
        cart.BillAddressId = Guid.NewGuid();
        cart.ShipAddressId = Guid.NewGuid();
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new OrderingAdvanceCheckout.AdvanceCheckout.Command(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var updated = await _dbContext.Set<Order>().FirstAsync(o => o.Id == cart.Id, TestContext.Current.CancellationToken);
        updated.CheckoutState.Should().Be(CheckoutState.Delivery);
    }

    [Fact(DisplayName = "Handler: Should fail when addresses not set")]
    public async Task Handle_ShouldFail_WhenAddressesMissing()
    {
        var cart = OrderExtensions.Create("USD", _userId, Guid.Empty).Value;
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new OrderingAdvanceCheckout.AdvanceCheckout.Command(), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return ok when no cart exists")]
    public async Task Handle_ShouldReturnOk_WhenNoCart()
    {
        var result = await _handler.Handle(new OrderingAdvanceCheckout.AdvanceCheckout.Command(), TestContext.Current.CancellationToken);
        result.IsSuccess.Should().BeTrue();
    }
}
