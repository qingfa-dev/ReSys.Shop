using Module.Ordering.Domain.Orders;
using OrderingDeleteCart = Module.Ordering.Features.Storefront.Cart.DeleteCart;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.DeleteCart;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "DeleteCart")]
public class DeleteCartTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly OrderingDeleteCart.DeleteCart.CommandHandler _handler;
    private readonly Guid _userId;

    public DeleteCartTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _userId = Guid.NewGuid();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserId).Returns(_userId.ToString());

        _handler = new OrderingDeleteCart.DeleteCart.CommandHandler(_dbContext, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should delete draft cart")]
    public async Task Handle_ShouldDeleteCart_WhenCartExists()
    {
        var cart = OrderExtensions.Create("USD", _userId, Guid.Empty).Value;
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new OrderingDeleteCart.DeleteCart.Command(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var deleted = await _dbContext.Set<Order>().IgnoreQueryFilters()
            .FirstAsync(o => o.Id == cart.Id, TestContext.Current.CancellationToken);
        deleted.IsDeleted.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return ok when no cart exists")]
    public async Task Handle_ShouldReturnOk_WhenNoCart()
    {
        var result = await _handler.Handle(new OrderingDeleteCart.DeleteCart.Command(), TestContext.Current.CancellationToken);
        result.IsSuccess.Should().BeTrue();
    }
}
