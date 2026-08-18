using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Create;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "CreateOrder")]
public class CreateOrderTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CreateOrder.CommandHandler _handler;

    public CreateOrderTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new CreateOrder.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create a draft order and return the detail response")]
    public async Task Handle_ShouldCreateDraftOrder_AndReturnResponse()
    {
        var result = await _handler.Handle(
            new CreateOrder.Command(new CreateOrder.Request { Currency = "USD" }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Currency.Should().Be("USD");
        result.Value.Status.Should().Be(OrderStatus.Draft);
        result.Value.UserId.Should().Be(Guid.Empty);

        var persisted = await _dbContext.Set<Order>().FirstAsync(o => o.Id == result.Value.Id);
        persisted.Should().NotBeNull();
        persisted.Currency.Should().Be("USD");
    }

    [Fact(DisplayName = "Validator: Should fail when currency is invalid")]
    public void Validator_ShouldFail_WhenCurrencyInvalid()
    {
        var validator = new CreateOrder.Validator();
        var command = new CreateOrder.Command(new CreateOrder.Request { Currency = "" });

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
