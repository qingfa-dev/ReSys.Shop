using Module.Ordering.Domain.Orders;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.CreateCart;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "CreateCart")]
public class CreateCartTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly global::Module.Ordering.Features.Storefront.Cart.CreateCart.CreateCart.CommandHandler _handler;

    public CreateCartTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());

        _handler = new global::Module.Ordering.Features.Storefront.Cart.CreateCart.CreateCart.CommandHandler(_dbContext, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create a new cart")]
    public async Task Handle_ShouldCreateCart()
    {
        var result = await _handler.Handle(
            new global::Module.Ordering.Features.Storefront.Cart.CreateCart.CreateCart.Command(),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBe(Guid.Empty);
        result.Value.Currency.Should().Be("USD");

        var saved = await _dbContext.Set<Order>().FirstAsync(o => o.Id == result.Value.Id, TestContext.Current.CancellationToken);
        saved.Status.Should().Be(OrderStatus.Draft);
    }
}
