using Module.Catalog.Domain.Variants;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.GetCartForShipping;

namespace Module.UnitTests.Ordering.Features.Storefront.GetCartForShipping;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "GetCartForShipping")]
public class GetCartForShippingTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetCartForShippingQueryHandler _handler;

    public GetCartForShippingTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetCartForShippingQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return shipping cost inputs for a draft cart")]
    public async Task Handle_ShouldReturnShippingInputs_ForDraftCart()
    {
        var ct = TestContext.Current.CancellationToken;
        var variant = VariantMethod.Create(Guid.NewGuid(), "SKU-1").Value;
        variant.Weight = 5m;

        var cart = OrderMethod.Create("USD", userId: Guid.NewGuid()).Value;
        cart.ShipAddressId = Guid.NewGuid();
        cart.ItemTotal = 30m;
        cart.Total = 30m;

        var lineItem = LineItemMethod.Create(cart.Id, variant.Id, 2, 15m).Value;
        cart.LineItems.Add(lineItem);

        _dbContext.Set<Variant>().Add(variant);
        _dbContext.Set<Order>().Add(cart);
        _dbContext.Set<LineItem>().Add(lineItem);
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new GetCartForShippingQuery { CartId = cart.Id },
            ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalWeight.Should().Be(10m);
        result.Value.TotalValue.Should().Be(30m);
        result.Value.ShipAddressId.Should().Be(cart.ShipAddressId);
        result.Value.Currency.Should().Be("USD");
    }

    [Fact(DisplayName = "Handler: Should return not found when cart missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(
            new GetCartForShippingQuery { CartId = Guid.NewGuid() },
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(OrderResult.Errors.NotFound(Guid.NewGuid()).Code);
    }
}
