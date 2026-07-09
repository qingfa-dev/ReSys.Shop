using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.Payments;
using Module.Payment.Domain.PaymentMethods;
using Module.Payment.Features.Storefront.Payment.CreateIntent;
using Module.Ordering.Domain.Orders;

using PaymentRecord = Module.Payment.Domain.Payments.PaymentRecord;

namespace Module.UnitTests.Payment.Features.Storefront.Payment.CreateIntent;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "CreatePaymentIntent")]
public class CreatePaymentIntentTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IPaymentGatewayActionProvider> _gatewayMock;
    private readonly CreatePaymentIntent.CommandHandler _handler;

    public CreatePaymentIntentTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [
            typeof(PaymentRecord).Assembly,
            typeof(Order).Assembly
        ];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("customer");
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());

        _gatewayMock = new Mock<IPaymentGatewayActionProvider>();
        _gatewayMock.Setup(x => x.AutoCapture).Returns(false);
        _gatewayMock.Setup(x => x.AuthorizeAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse(true, "Authorized"));

        _handler = new CreatePaymentIntent.CommandHandler(_dbContext, _currentUserMock.Object, _gatewayMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create payment intent for an order")]
    public async Task Handle_ShouldCreatePayment_WhenOrderExists()
    {
        // Arrange: Create order
        var userId = Guid.Parse(_currentUserMock.Object.UserId!);
        var order = OrderExtensions.Create("USD", userId, Guid.NewGuid()).Value;
        order.Status = OrderStatus.Placed;
        order.Total = 100.00m;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Arrange: Create a payment method
        var pm = new PaymentMethod { Name = "Credit Card", Code = "credit_card", ProviderType = "stripe" };
        _dbContext.Set<PaymentMethod>().Add(pm);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new CreatePaymentIntent.Command(order.Id),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "Handler: Should return failure when gateway declines authorization")]
    public async Task Handle_ShouldReturnFailure_WhenGatewayDeclines()
    {
        _gatewayMock.Setup(x => x.AuthorizeAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.BadRequest("Gateway.Declined", "Card declined."));

        var userId = Guid.Parse(_currentUserMock.Object.UserId!);
        var order = OrderExtensions.Create("USD", userId, Guid.NewGuid()).Value;
        order.Status = OrderStatus.Placed;
        order.Total = 100.00m;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var pm = new PaymentMethod { Name = "Credit Card", Code = "credit_card", ProviderType = "stripe" };
        _dbContext.Set<PaymentMethod>().Add(pm);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new CreatePaymentIntent.Command(order.Id),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when order not found")]
    public async Task Handle_ShouldReturnFailure_WhenOrderNotFound()
    {
        var result = await _handler.Handle(
            new CreatePaymentIntent.Command(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(OrderResult.Errors.NotFound(Guid.NewGuid()).Code);
    }
}
