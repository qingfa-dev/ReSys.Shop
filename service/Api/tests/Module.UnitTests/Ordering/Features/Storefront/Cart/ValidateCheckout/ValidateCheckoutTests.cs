using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.ValidateCheckout;
using ValidateCheckoutFeature = Module.Ordering.Features.Storefront.Cart.ValidateCheckout.ValidateCheckout;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.ValidateCheckout;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "ValidateCheckout")]
public class ValidateCheckoutTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly ValidateCheckoutFeature.CommandHandler _handler;

    public ValidateCheckoutTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());

        _handler = new ValidateCheckoutFeature.CommandHandler(_dbContext, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should succeed when cart is complete for checkout")]
    public async Task Handle_ShouldSucceed_WhenCartComplete()
    {
        var ct = TestContext.Current.CancellationToken;
        var cart = CreateCompleteCart();

        var result = await _handler.Handle(new ValidateCheckoutFeature.Command(), ct);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should fail when addresses are missing")]
    public async Task Handle_ShouldFail_WhenAddressesMissing()
    {
        var ct = TestContext.Current.CancellationToken;
        var cart = CreateCompleteCart();
        cart.BillAddressId = null;
        cart.ShipAddressId = null;
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new ValidateCheckoutFeature.Command(), ct);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(OrderResult.Errors.AddressRequired.Code);
    }

    private Order CreateCompleteCart()
    {
        var userId = Guid.Parse(_currentUserMock.Object.UserId!);
        var cart = OrderMethod.Create("USD", userId: userId).Value;
        cart.BillAddressId = Guid.NewGuid();
        cart.ShipAddressId = Guid.NewGuid();
        cart.ShippingMethodId = Guid.NewGuid();
        cart.PaymentMethodId = Guid.NewGuid();
        cart.Email = "test@example.com";

        var lineItem = LineItemMethod.Create(cart.Id, Guid.NewGuid(), 2, 15m).Value;
        cart.LineItems.Add(lineItem);

        _dbContext.Set<Order>().Add(cart);
        _dbContext.Set<LineItem>().Add(lineItem);
        _dbContext.SaveChanges();
        return cart;
    }
}
