using Module.Ordering.Domain.Orders;
using OrderingUpdateCheckout = Module.Ordering.Features.Storefront.Cart.UpdateCheckout;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.UpdateCheckout;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "UpdateCheckout")]
public class UpdateCheckoutTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly OrderingUpdateCheckout.UpdateCheckout.CommandHandler _handler;
    private readonly Guid _userId;

    public UpdateCheckoutTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _userId = Guid.NewGuid();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserId).Returns(_userId.ToString());

        _handler = new OrderingUpdateCheckout.UpdateCheckout.CommandHandler(_dbContext, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should update address details on a cart already in the Address state")]
    public async Task Handle_ShouldUpdateCheckout_WhenCartAtAddressState()
    {
        // Saving both addresses advances the cart from Address to Delivery — REQ-005.
        var cart = OrderMethod.Create("USD", _userId, Guid.Empty).Value;
        cart.CheckoutState = CheckoutState.Address;
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new OrderingUpdateCheckout.UpdateCheckout.Request
        {
            Email = "customer@example.com",
            ShipAddressId = Guid.NewGuid(),
            BillAddressId = Guid.NewGuid(),
        };

        var result = await _handler.Handle(
            new OrderingUpdateCheckout.UpdateCheckout.Command(request),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var cartAfter = await _dbContext.Set<Order>()
            .FirstAsync(o => o.Id == cart.Id, TestContext.Current.CancellationToken);
        cartAfter.Email.Should().Be("customer@example.com");
        cartAfter.CheckoutState.Should().Be(CheckoutState.Delivery);
    }
}
