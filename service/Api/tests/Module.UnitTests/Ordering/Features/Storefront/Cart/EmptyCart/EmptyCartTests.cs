using Module.Ordering.Domain.Orders;
using OrderingEmptyCart = Module.Ordering.Features.Storefront.Cart.EmptyCart;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.EmptyCart;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "EmptyCart")]
public class EmptyCartTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly OrderingEmptyCart.EmptyCart.CommandHandler _handler;
    private readonly Guid _userId;

    public EmptyCartTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _userId = Guid.NewGuid();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserId).Returns(_userId.ToString());

        _handler = new OrderingEmptyCart.EmptyCart.CommandHandler(_dbContext, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should empty cart")]
    public async Task Handle_ShouldEmptyCart_WhenCartExists()
    {
        var cart = OrderExtensions.Create("USD", _userId, Guid.Empty).Value;
        cart.LineItems.Add(new Module.Ordering.Domain.LineItems.LineItem
        {
            Id = Guid.NewGuid(), OrderId = cart.Id, VariantId = Guid.NewGuid(),
            Quantity = 1, Price = 10m, Total = 10m, Currency = "USD"
        });
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new OrderingEmptyCart.EmptyCart.Command(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var cartAfter = await _dbContext.Set<Order>()
            .Include(x => x.LineItems)
            .FirstAsync(o => o.Id == cart.Id, TestContext.Current.CancellationToken);
        cartAfter.LineItems.Should().BeEmpty();
        cartAfter.ItemTotal.Should().Be(0);
    }

    [Fact(DisplayName = "Handler: Should return ok when no cart exists")]
    public async Task Handle_ShouldReturnOk_WhenNoCart()
    {
        var result = await _handler.Handle(new OrderingEmptyCart.EmptyCart.Command(), TestContext.Current.CancellationToken);
        result.IsSuccess.Should().BeTrue();
    }
}
