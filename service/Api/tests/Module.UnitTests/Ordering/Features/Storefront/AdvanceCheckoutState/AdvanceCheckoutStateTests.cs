using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.AdvanceCheckoutState;

namespace Module.UnitTests.Ordering.Features.Storefront.AdvanceCheckoutState;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "AdvanceCheckoutState")]
public class AdvanceCheckoutStateTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly AdvanceCheckoutStateCommandHandler _handler;

    public AdvanceCheckoutStateTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new AdvanceCheckoutStateCommandHandler(_dbContext);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Handler: records the selected payment method when advancing to PickPaymentMethod")]
    public async Task Handle_WithPaymentMethodId_RecordsMethod()
    {
        var cart = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        cart.AdvanceCheckoutState(CheckoutState.PickDeliveryMethod);
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var methodId = Guid.NewGuid();
        var result = await _handler.Handle(new AdvanceCheckoutStateCommand
        {
            CartId = cart.Id,
            TargetState = CheckoutState.PickPaymentMethod,
            PaymentMethodId = methodId
        }, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var updated = await _dbContext.Set<Order>().FirstAsync(o => o.Id == cart.Id);
        updated.CheckoutState.Should().Be(CheckoutState.PickPaymentMethod);
        updated.PaymentMethodId.Should().Be(methodId);
        updated.HasPayementMethod().Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: leaves PaymentMethodId untouched when not provided")]
    public async Task Handle_WithoutPaymentMethodId_LeavesUnchanged()
    {
        var cart = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        cart.AdvanceCheckoutState(CheckoutState.PickDeliveryMethod);
        cart.PaymentMethodId = Guid.NewGuid();
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new AdvanceCheckoutStateCommand
        {
            CartId = cart.Id,
            TargetState = CheckoutState.PickPaymentMethod
        }, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var updated = await _dbContext.Set<Order>().FirstAsync(o => o.Id == cart.Id);
        updated.PaymentMethodId.Should().Be(cart.PaymentMethodId);
    }
}
