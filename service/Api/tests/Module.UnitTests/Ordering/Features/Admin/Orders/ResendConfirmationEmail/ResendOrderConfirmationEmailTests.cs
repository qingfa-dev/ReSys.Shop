using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.ResendConfirmationEmail;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.ResendConfirmationEmail;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "ResendOrderConfirmationEmail")]
public class ResendOrderConfirmationEmailTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ResendOrderConfirmationEmail.CommandHandler _handler;

    public ResendOrderConfirmationEmailTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new ResendOrderConfirmationEmail.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should succeed for placed order")]
    public async Task Handle_ShouldSucceed_WhenPlaced()
    {
        var order = OrderExtensions.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty).Value;
        order.Status = OrderStatus.Placed;
        order.CompletedAtUtc = DateTimeOffset.UtcNow;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new ResendOrderConfirmationEmail.Command(order.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return not found when order missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(
            new ResendOrderConfirmationEmail.Command(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be(OrderResult.Errors.NotFound(Guid.NewGuid()).Code);
    }
}
