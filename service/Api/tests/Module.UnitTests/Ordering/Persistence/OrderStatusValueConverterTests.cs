using Shared.Application.Domain.Orders;

using Module.Ordering.Domain.Orders;

namespace Module.UnitTests.Ordering.Persistence;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "OrderStatusValueConverter")]
public class OrderStatusValueConverterTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;

    public OrderStatusValueConverterTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "CheckoutState: legacy 'Delivery'/'Payment' map to Pick* names")]
    public void CheckoutState_LegacyStrings_MapToNewNames()
    {
        var converter = _dbContext.Model.FindEntityType(typeof(Order))!
            .FindProperty(nameof(Order.CheckoutState))!
            .GetValueConverter()!;

        converter.ConvertFromProvider("Delivery").Should().Be(CheckoutState.PickDeliveryMethod);
        converter.ConvertFromProvider("Payment").Should().Be(CheckoutState.PickPaymentMethod);
        converter.ConvertFromProvider("PickDeliveryMethod").Should().Be(CheckoutState.PickDeliveryMethod);
        converter.ConvertToProvider(CheckoutState.PickPaymentMethod).Should().Be("PickPaymentMethod");
    }

    [Fact(DisplayName = "PaymentState: legacy snake_case maps to enum members")]
    public void PaymentState_LegacyStrings_MapToEnum()
    {
        var converter = _dbContext.Model.FindEntityType(typeof(Order))!
            .FindProperty(nameof(Order.PaymentState))!
            .GetValueConverter()!;

        converter.ConvertFromProvider("balance_due").Should().Be(OrderPaymentState.BalanceDue);
        converter.ConvertFromProvider("paid").Should().Be(OrderPaymentState.Paid);
        converter.ConvertFromProvider("BalanceDue").Should().Be(OrderPaymentState.BalanceDue);
        converter.ConvertToProvider(OrderPaymentState.CreditOwed).Should().Be("CreditOwed");
    }

    [Fact(DisplayName = "FulfillmentState: legacy lowercase maps to derived members")]
    public void FulfillmentState_LegacyStrings_MapToEnum()
    {
        var converter = _dbContext.Model.FindEntityType(typeof(Order))!
            .FindProperty(nameof(Order.ShipmentState))!
            .GetValueConverter()!;

        converter.ConvertFromProvider("ready").Should().Be(ShipmentState.Pending);
        converter.ConvertFromProvider("backorder").Should().Be(ShipmentState.Pending);
        converter.ConvertFromProvider("pending").Should().Be(ShipmentState.Pending);
        converter.ConvertFromProvider("Shipped").Should().Be(ShipmentState.Shipped);
        converter.ConvertToProvider(ShipmentState.Delivered).Should().Be("Delivered");
    }

    [Fact(DisplayName = "Order: canonical round-trip preserves enums through the model")]
    public async Task Order_StatusRoundTrip_PreservesEnums()
    {
        var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        order.CheckoutState = CheckoutState.PickPaymentMethod;
        order.PaymentState = OrderPaymentState.BalanceDue;
        order.ShipmentState = ShipmentState.Pending;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var loaded = await _dbContext.Set<Order>().FirstAsync(o => o.Id == order.Id);
        loaded.CheckoutState.Should().Be(CheckoutState.PickPaymentMethod);
        loaded.PaymentState.Should().Be(OrderPaymentState.BalanceDue);
        loaded.ShipmentState.Should().Be(ShipmentState.Pending);
    }
}
