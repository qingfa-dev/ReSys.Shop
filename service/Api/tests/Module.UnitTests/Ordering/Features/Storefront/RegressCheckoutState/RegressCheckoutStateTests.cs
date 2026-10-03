using Microsoft.EntityFrameworkCore;

using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.RegressCheckoutState;

namespace Module.UnitTests.Ordering.Features.Storefront.RegressCheckoutState;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "RegressCheckoutState")]
public class RegressCheckoutStateTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly RegressCheckoutStateCommandHandler _handler;

    public RegressCheckoutStateTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new RegressCheckoutStateCommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should regress checkout state and clear payment method")]
    public async Task Handle_ShouldRegressState_AndClearPaymentMethod()
    {
        var ct = TestContext.Current.CancellationToken;
        var cart = OrderMethod.Create("USD", userId: Guid.NewGuid()).Value;
        cart.AdvanceCheckoutState(CheckoutState.PickDeliveryMethod);
        cart.AdvanceCheckoutState(CheckoutState.PickPaymentMethod);
        cart.PaymentMethodId = Guid.NewGuid();
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new RegressCheckoutStateCommand
        {
            CartId = cart.Id,
            TargetState = CheckoutState.PickDeliveryMethod
        }, ct);

        result.IsSuccess.Should().BeTrue();

        var updated = await _dbContext.Set<Order>().FirstAsync(o => o.Id == cart.Id);
        updated.CheckoutState.Should().Be(CheckoutState.PickDeliveryMethod);
        updated.PaymentMethodId.Should().BeNull();
    }

    [Fact(DisplayName = "Handler: Should return invalid transition when target is not a prior step")]
    public async Task Handle_ShouldFail_WhenTargetNotPriorStep()
    {
        var ct = TestContext.Current.CancellationToken;
        var cart = OrderMethod.Create("USD", userId: Guid.NewGuid()).Value;
        cart.AdvanceCheckoutState(CheckoutState.PickDeliveryMethod);
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new RegressCheckoutStateCommand
        {
            CartId = cart.Id,
            TargetState = CheckoutState.PickPaymentMethod
        }, ct);

        result.IsFailure.Should().BeTrue();
    }
}
