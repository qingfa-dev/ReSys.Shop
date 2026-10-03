using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.AssociateCart;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.AssociateCart;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "AssociateCartWithUser")]
public class AssociateCartWithUserTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly AssociateCartWithUser.CommandHandler _handler;
    private readonly Guid _userId;

    public AssociateCartWithUserTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _userId = Guid.NewGuid();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserId).Returns(_userId.ToString());

        _handler = new AssociateCartWithUser.CommandHandler(_dbContext, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should merge guest cart into user cart")]
    public async Task Handle_ShouldMergeGuestCartIntoUserCart()
    {
        // Arrange: user cart with 1 item
        var userCart = OrderMethod.Create("USD", _userId).Value;
        var lineItem1 = LineItemMethod.Create(userCart.Id, Guid.NewGuid(), 1, 10m).Value;
        userCart.LineItems.Add(lineItem1);
        _dbContext.Set<Order>().Add(userCart);

        // Arrange: guest cart with 2 items (one matching variant)
        var guestCart = OrderMethod.Create("USD", null).Value;
        var matchingItem = LineItemMethod.Create(guestCart.Id, lineItem1.VariantId, 3, 10m).Value;
        var newItem = LineItemMethod.Create(guestCart.Id, Guid.NewGuid(), 2, 20m).Value;
        guestCart.LineItems.Add(matchingItem);
        guestCart.LineItems.Add(newItem);
        _dbContext.Set<Order>().Add(guestCart);

        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new AssociateCartWithUser.Command(new AssociateCartWithUser.Request { GuestOrderId = guestCart.Id }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ItemCount.Should().Be(6); // merged: qty 4 + qty 2

        var persistedUserCart = await _dbContext.Set<Order>()
            .Include(o => o.LineItems)
            .FirstAsync(o => o.Id == userCart.Id, TestContext.Current.CancellationToken);

        persistedUserCart.LineItems.Should().HaveCount(2);
        persistedUserCart.LineItems.First(li => li.VariantId == lineItem1.VariantId).Quantity.Should().Be(4); // 1 + 3
    }

    [Fact(DisplayName = "Handler: Should return not found when guest cart missing")]
    public async Task Handle_ShouldReturnNotFound_WhenGuestCartMissing()
    {
        var result = await _handler.Handle(
            new AssociateCartWithUser.Command(new AssociateCartWithUser.Request { GuestOrderId = Guid.NewGuid() }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(OrderResult.Errors.NotFound(Guid.NewGuid()).Code);
    }
}
